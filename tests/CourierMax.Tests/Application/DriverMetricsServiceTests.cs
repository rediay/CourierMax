using Moq;
using FluentAssertions;
using CourierMax.Application.Services;
using CourierMax.Domain.Entities;
using CourierMax.Domain.Enums;
using CourierMax.Domain.Interfaces;
using CourierMax.Domain.ValueObjects;

namespace CourierMax.Tests.Application.Services;

public class DriverMetricsServiceTests
{
    private readonly Mock<IShipmentRepository> _mockShipmentRepo;
    private readonly Mock<IDriverRepository> _mockDriverRepo;
    private readonly DriverMetricsService _service;

    public DriverMetricsServiceTests()
    {
        _mockShipmentRepo = new Mock<IShipmentRepository>();
        _mockDriverRepo = new Mock<IDriverRepository>();
        _service = new DriverMetricsService(_mockShipmentRepo.Object, _mockDriverRepo.Object);
    }

    private static Shipment CreateAssignedShipment(decimal weightKg, ShipmentStatus finalStatus, DateTime assignedAt, DateTime? deliveredAt = null)
    {
        var shipment = new Shipment(
            "Juan Perez", new Phone("3001234567"), new Address("Calle 123"),
            "Maria Lopez", new Phone("3102345678"), new Address("Carrera 456"),
            new Weight(weightKg), new Dimensions(30, 20, 10),
            PackageType.Paquete, ServiceType.Estandar, "Bogotá", "Medellín");

        shipment.Assign(1, 1, "operator", 15000m);
        typeof(ShipmentStatusHistory).GetProperty(nameof(ShipmentStatusHistory.ChangedAt))!
            .SetValue(shipment.StatusHistories.Last(), assignedAt);

        if (finalStatus == ShipmentStatus.EN_TRANSITO)
        {
            shipment.MarkInTransit("operator");
        }
        else if (finalStatus == ShipmentStatus.ENTREGADO)
        {
            shipment.MarkInTransit("operator");
            shipment.Deliver("operator");
            typeof(ShipmentStatusHistory).GetProperty(nameof(ShipmentStatusHistory.ChangedAt))!
                .SetValue(shipment.StatusHistories.Last(), deliveredAt ?? assignedAt.AddDays(1));
        }
        else if (finalStatus == ShipmentStatus.CANCELADO)
        {
            shipment.Cancel("Cliente canceló", "operator");
        }

        return shipment;
    }

    [Fact]
    public async Task GetEfficiencyReportAsync_ComputesAggregatesCorrectly()
    {
        var driver = new Driver("Juan Pérez", null, null);
        var assignedAt = new DateTime(2026, 6, 1);

        var delivered = CreateAssignedShipment(10, ShipmentStatus.ENTREGADO, assignedAt, assignedAt.AddDays(2));
        var inTransit = CreateAssignedShipment(5, ShipmentStatus.EN_TRANSITO, assignedAt);
        var cancelled = CreateAssignedShipment(3, ShipmentStatus.CANCELADO, assignedAt);

        _mockDriverRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(driver);
        _mockShipmentRepo.Setup(r => r.GetByDriverIdAsync(1))
            .ReturnsAsync(new[] { delivered, inTransit, cancelled });

        var report = await _service.GetEfficiencyReportAsync(1);

        report.TotalAssigned.Should().Be(3);
        report.TotalDelivered.Should().Be(1);
        report.TotalCancelled.Should().Be(1);
        report.TotalInTransit.Should().Be(1);
        report.AverageDeliveryDays.Should().Be(2);
        report.TotalWeightTransportedKg.Should().Be(15); // delivered(10) + in transit(5), cancelled excluded
    }

    [Fact]
    public async Task GetEfficiencyReportAsync_NonExistingDriver_ThrowsKeyNotFoundException()
    {
        _mockDriverRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Driver?)null);

        Func<Task> act = () => _service.GetEfficiencyReportAsync(999);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GetEfficiencyReportAsync_NoShipments_ReturnsZeroedReport()
    {
        var driver = new Driver("Carlos Ruiz", null, null);
        _mockDriverRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(driver);
        _mockShipmentRepo.Setup(r => r.GetByDriverIdAsync(1)).ReturnsAsync(Array.Empty<Shipment>());

        var report = await _service.GetEfficiencyReportAsync(1);

        report.TotalAssigned.Should().Be(0);
        report.AverageDeliveryDays.Should().Be(0);
        report.OnTimeDeliveryPercentage.Should().Be(0);
        report.TotalWeightTransportedKg.Should().Be(0);
    }
}
