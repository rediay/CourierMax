using CourierMax.Application.DTOs;

namespace CourierMax.Application.Services;

public interface IShipmentService
{
    Task<ShipmentResponse> CreateAsync(CreateShipmentRequest request);
    Task<ShipmentResponse?> GetByTrackingCodeAsync(string trackingCode);
}
