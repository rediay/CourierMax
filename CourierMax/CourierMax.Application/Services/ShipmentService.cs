using CourierMax.Application.DTOs;
using CourierMax.Domain.Entities;
using CourierMax.Domain.Interfaces;

namespace CourierMax.Application.Services;

public class ShipmentService : IShipmentService
{
    private readonly IShipmentRepository _shipmentRepository;

    public ShipmentService(IShipmentRepository shipmentRepository)
    {
        _shipmentRepository = shipmentRepository;
    }

    public async Task<ShipmentResponse> CreateAsync(CreateShipmentRequest request)
    {
        var shipment = new Shipment(
            request.SenderName,
            new Domain.ValueObjects.Phone(request.SenderPhone),
            new Domain.ValueObjects.Address(request.SenderAddress),
            request.RecipientName,
            new Domain.ValueObjects.Phone(request.RecipientPhone),
            new Domain.ValueObjects.Address(request.RecipientAddress),
            new Domain.ValueObjects.Weight(request.PackageWeight),
            new Domain.ValueObjects.Dimensions(request.PackageLength, request.PackageWidth, request.PackageHeight),
            request.PackageType,
            request.ServiceType,
            request.Origin,
            request.Destination);

        await _shipmentRepository.AddAsync(shipment);

        return MapToResponse(shipment);
    }

    public async Task<ShipmentResponse?> GetByTrackingCodeAsync(string trackingCode)
    {
        var shipment = await _shipmentRepository.GetByTrackingCodeAsync(trackingCode);
        return shipment is null ? null : MapToResponse(shipment);
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
            TotalCost = shipment.TotalCost,
            CreatedAt = shipment.CreatedAt
        };
    }
}
