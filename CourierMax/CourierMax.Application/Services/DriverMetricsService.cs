using CourierMax.Application.DTOs;
using CourierMax.Domain.Enums;
using CourierMax.Domain.Interfaces;
using CourierMax.Domain.Services;
using Microsoft.Extensions.Logging;

namespace CourierMax.Application.Services;

public class DriverMetricsService : IDriverMetricsService
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IDriverRepository _driverRepository;
    private readonly ILogger<DriverMetricsService> _logger;

    public DriverMetricsService(
        IShipmentRepository shipmentRepository,
        IDriverRepository driverRepository,
        ILogger<DriverMetricsService> logger)
    {
        _shipmentRepository = shipmentRepository;
        _driverRepository = driverRepository;
        _logger = logger;
    }

    public async Task<DriverEfficiencyReportResponse> GetEfficiencyReportAsync(int driverId)
    {
        var driver = await _driverRepository.GetByIdAsync(driverId)
            ?? throw new KeyNotFoundException($"Driver with id {driverId} not found.");

        var shipments = (await _shipmentRepository.GetByDriverIdAsync(driverId)).ToList();

        var delivered = shipments.Where(s => s.Status == ShipmentStatus.ENTREGADO).ToList();
        var cancelled = shipments.Count(s => s.Status == ShipmentStatus.CANCELADO);
        var inTransit = shipments.Count(s => s.Status == ShipmentStatus.EN_TRANSITO);

        var deliveryDurationsDays = new List<double>();
        var onTimeDeliveries = 0;

        foreach (var shipment in delivered)
        {
            var assignedAt = shipment.StatusHistories
                .FirstOrDefault(h => h.NewStatus == ShipmentStatus.ASIGNADO)?.ChangedAt;
            var deliveredAt = shipment.StatusHistories
                .FirstOrDefault(h => h.NewStatus == ShipmentStatus.ENTREGADO)?.ChangedAt;

            if (assignedAt.HasValue && deliveredAt.HasValue)
                deliveryDurationsDays.Add((deliveredAt.Value - assignedAt.Value).TotalDays);

            if (!SlaPolicy.IsOverdue(shipment, deliveredAt ?? DateTime.UtcNow))
                onTimeDeliveries++;
        }

        var weightTransportedKg = shipments
            .Where(s => s.Status is ShipmentStatus.ASIGNADO or ShipmentStatus.EN_TRANSITO or ShipmentStatus.ENTREGADO)
            .Sum(s => s.PackageWeight.Kg);

        _logger.LogInformation(
            "Efficiency report generated for driver {DriverId} ({DriverName}): {TotalAssigned} envíos, {TotalDelivered} entregados",
            driver.Id, driver.Name, shipments.Count, delivered.Count);

        return new DriverEfficiencyReportResponse
        {
            DriverId = driver.Id,
            DriverName = driver.Name,
            TotalAssigned = shipments.Count,
            TotalDelivered = delivered.Count,
            TotalCancelled = cancelled,
            TotalInTransit = inTransit,
            AverageDeliveryDays = deliveryDurationsDays.Count > 0
                ? Math.Round(deliveryDurationsDays.Average(), 2)
                : 0,
            OnTimeDeliveryPercentage = delivered.Count > 0
                ? Math.Round((double)onTimeDeliveries / delivered.Count * 100, 2)
                : 0,
            TotalWeightTransportedKg = weightTransportedKg
        };
    }
}
