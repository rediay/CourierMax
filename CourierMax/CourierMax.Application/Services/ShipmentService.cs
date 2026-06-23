using CourierMax.Application.DTOs;
using CourierMax.Domain.Entities;
using CourierMax.Domain.Enums;
using CourierMax.Domain.Interfaces;
using CourierMax.Domain.Reference;
using CourierMax.Domain.Services;
using CourierMax.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace CourierMax.Application.Services;

public class ShipmentService : IShipmentService
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IDriverRepository _driverRepository;
    private readonly ICostCalculationService _costCalculationService;
    private readonly ILogger<ShipmentService> _logger;

    private const int MaxTrackingCodeGenerationAttempts = 10;

    public ShipmentService(
        IShipmentRepository shipmentRepository,
        IVehicleRepository vehicleRepository,
        IDriverRepository driverRepository,
        ICostCalculationService costCalculationService,
        ILogger<ShipmentService> logger)
    {
        _shipmentRepository = shipmentRepository;
        _vehicleRepository = vehicleRepository;
        _driverRepository = driverRepository;
        _costCalculationService = costCalculationService;
        _logger = logger;
    }

    public async Task<ShipmentResponse> CreateAsync(CreateShipmentRequest request)
    {
        if (!ReferenceCities.IsValid(request.Origin))
            throw new ArgumentException($"Origin city '{request.Origin}' is not a valid reference city.");

        if (!ReferenceCities.IsValid(request.Destination))
            throw new ArgumentException($"Destination city '{request.Destination}' is not a valid reference city.");

        var trackingCode = await GenerateUniqueTrackingCodeAsync();

        var shipment = new Shipment(
            request.SenderName,
            new Phone(request.SenderPhone),
            new Address(request.SenderAddress),
            request.RecipientName,
            new Phone(request.RecipientPhone),
            new Address(request.RecipientAddress),
            new Weight(request.PackageWeight),
            new Dimensions(request.PackageLength, request.PackageWidth, request.PackageHeight),
            request.PackageType,
            request.ServiceType,
            request.Origin,
            request.Destination,
            trackingCode);

        await _shipmentRepository.AddAsync(shipment);

        _logger.LogInformation(
            "Shipment {TrackingCode} created ({Origin} -> {Destination}, {ServiceType}, {PackageType}, {WeightKg}kg)",
            shipment.TrackingCode, shipment.Origin, shipment.Destination,
            shipment.ServiceType, shipment.PackageType, shipment.PackageWeight.Kg);

        return MapToResponse(shipment);
    }

    public async Task<ShipmentResponse?> GetByTrackingCodeAsync(string trackingCode)
    {
        var shipment = await _shipmentRepository.GetByTrackingCodeAsync(trackingCode);
        return shipment is null ? null : MapToResponse(shipment);
    }

    public async Task<ShipmentResponse> AssignAsync(int id, AssignRequest request)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(id);
        if (shipment is null)
            throw new KeyNotFoundException($"Shipment with id {id} not found.");

        var weightKg = shipment.PackageWeight.Kg;
        var volumeM3 = shipment.PackageDimensions.VolumeM3;

        Vehicle vehicle;
        Driver driver;

        if (request.DriverId.HasValue)
        {
            driver = await _driverRepository.GetByIdAsync(request.DriverId.Value)
                ?? throw new KeyNotFoundException($"Driver with id {request.DriverId} not found.");

            if (!driver.IsActive)
                throw new InvalidOperationException($"Driver '{driver.Name}' is not active and cannot be assigned.");

            vehicle = await _vehicleRepository.GetByDriverIdAsync(driver.Id)
                ?? throw new InvalidOperationException($"Driver '{driver.Name}' has no vehicle assigned.");

            if (!vehicle.HasCapacityFor(weightKg, volumeM3))
                throw new InvalidOperationException(
                    $"Vehicle {vehicle.Plate} does not have enough capacity for this shipment " +
                    $"(weight: {weightKg}kg, volume: {volumeM3:F4}m3).");
        }
        else
        {
            var candidates = (await _vehicleRepository.GetAllWithActiveDriverAsync())
                .Where(v => v.HasCapacityFor(weightKg, volumeM3))
                .OrderBy(v => v.CurrentWeightKg + v.CurrentVolumeM3)
                .ToList();

            vehicle = candidates.FirstOrDefault()
                ?? throw new InvalidOperationException("No active vehicle has enough capacity for this shipment.");
            driver = vehicle.Driver!;
        }

        var cost = await _costCalculationService.CalculateAsync(
            shipment.Origin, shipment.Destination,
            weightKg, shipment.ServiceType, shipment.PackageType);

        shipment.Assign(vehicle.Id, driver.Id, request.ChangedBy, cost.TotalCost);
        vehicle.LoadCargo(weightKg, volumeM3);

        await _shipmentRepository.UpdateAsync(shipment);
        await _vehicleRepository.UpdateAsync(vehicle);

        _logger.LogInformation(
            "Shipment {TrackingCode} assigned to driver {DriverId} / vehicle {Plate} by {ChangedBy} (cost: {TotalCost})",
            shipment.TrackingCode, driver.Id, vehicle.Plate, request.ChangedBy, cost.TotalCost);

        return MapToResponse(shipment);
    }

    public async Task<CostEstimateResponse> GetCostEstimateAsync(string trackingCode)
    {
        var shipment = await _shipmentRepository.GetByTrackingCodeAsync(trackingCode);
        if (shipment is null)
            throw new KeyNotFoundException($"Shipment with tracking code '{trackingCode}' not found.");

        return await _costCalculationService.CalculateAsync(
            shipment.Origin, shipment.Destination,
            shipment.PackageWeight.Kg, shipment.ServiceType, shipment.PackageType);
    }

    public async Task<ShipmentResponse> UpdateStatusAsync(int id, UpdateStatusRequest request)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(id);
        if (shipment is null)
            throw new KeyNotFoundException($"Shipment with id {id} not found.");

        if (!Enum.TryParse<ShipmentStatus>(request.NewStatus, true, out var newStatus))
            throw new ArgumentException($"'{request.NewStatus}' is not a valid shipment status.");

        switch (newStatus)
        {
            case ShipmentStatus.ASIGNADO:
                throw new InvalidOperationException("Use the assign endpoint to assign a shipment.");
            case ShipmentStatus.EN_TRANSITO:
                shipment.MarkInTransit(request.ChangedBy);
                break;
            case ShipmentStatus.ENTREGADO:
                shipment.Deliver(request.ChangedBy);
                break;
            case ShipmentStatus.CANCELADO:
                await CancelAndReleaseCapacityAsync(shipment, request);
                break;
            default:
                throw new InvalidOperationException($"Cannot transition to status {newStatus}.");
        }

        await _shipmentRepository.UpdateAsync(shipment);

        _logger.LogInformation(
            "Shipment {TrackingCode} transitioned to {NewStatus} by {ChangedBy}",
            shipment.TrackingCode, shipment.Status, request.ChangedBy);

        return MapToResponse(shipment);
    }

    public async Task<IEnumerable<ShipmentHistoryResponse>> GetHistoryAsync(int id)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(id);
        if (shipment is null)
            throw new KeyNotFoundException($"Shipment with id {id} not found.");

        return shipment.StatusHistories.Select(h => new ShipmentHistoryResponse
        {
            Id = h.Id,
            PreviousStatus = h.PreviousStatus?.ToString(),
            NewStatus = h.NewStatus.ToString(),
            ChangedAt = h.ChangedAt,
            Reason = h.Reason,
            ChangedBy = h.ChangedBy
        });
    }

    public async Task<IEnumerable<ShipmentResponse>> GetOverdueShipmentsAsync(DateTime from, DateTime to)
    {
        var shipments = await _shipmentRepository.GetByCreatedDateRangeAsync(from, to);
        var now = DateTime.UtcNow;

        return shipments
            .Where(s => SlaPolicy.IsOverdue(s, now))
            .Select(MapToResponse);
    }

    private async Task CancelAndReleaseCapacityAsync(Shipment shipment, UpdateStatusRequest request)
    {
        var vehicleId = shipment.VehicleId;
        var weightKg = shipment.PackageWeight.Kg;
        var volumeM3 = shipment.PackageDimensions.VolumeM3;

        shipment.Cancel(request.Reason ?? string.Empty, request.ChangedBy);

        if (vehicleId.HasValue)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId.Value);
            if (vehicle is not null)
            {
                vehicle.ReleaseCargo(weightKg, volumeM3);
                await _vehicleRepository.UpdateAsync(vehicle);

                _logger.LogInformation(
                    "Released {WeightKg}kg / {VolumeM3}m3 from vehicle {Plate} after cancelling shipment {TrackingCode}",
                    weightKg, volumeM3, vehicle.Plate, shipment.TrackingCode);
            }
        }
    }

    private async Task<TrackingCode> GenerateUniqueTrackingCodeAsync()
    {
        for (var attempt = 0; attempt < MaxTrackingCodeGenerationAttempts; attempt++)
        {
            var candidate = TrackingCode.Generate();
            if (!await _shipmentRepository.TrackingCodeExistsAsync(candidate.ToString()))
                return candidate;
        }

        throw new InvalidOperationException("Could not generate a unique tracking code after several attempts.");
    }

    private static ShipmentResponse MapToResponse(Shipment shipment)
    {
        return new ShipmentResponse
        {
            Id = shipment.Id,
            TrackingCode = shipment.TrackingCode.ToString(),
            SenderName = shipment.SenderName,
            SenderPhone = shipment.SenderPhone.ToString(),
            SenderAddress = shipment.SenderAddress.ToString(),
            RecipientName = shipment.RecipientName,
            RecipientPhone = shipment.RecipientPhone.ToString(),
            RecipientAddress = shipment.RecipientAddress.ToString(),
            PackageWeight = shipment.PackageWeight.Kg,
            PackageLength = shipment.PackageDimensions.LengthCm,
            PackageWidth = shipment.PackageDimensions.WidthCm,
            PackageHeight = shipment.PackageDimensions.HeightCm,
            PackageType = shipment.PackageType.ToString(),
            ServiceType = shipment.ServiceType.ToString(),
            Origin = shipment.Origin,
            Destination = shipment.Destination,
            Status = shipment.Status.ToString(),
            VehicleId = shipment.VehicleId,
            DriverId = shipment.DriverId,
            TotalCost = shipment.TotalCost,
            CreatedAt = shipment.CreatedAt
        };
    }
}
