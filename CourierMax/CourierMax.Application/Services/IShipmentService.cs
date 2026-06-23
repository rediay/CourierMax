using CourierMax.Application.DTOs;

namespace CourierMax.Application.Services;

public interface IShipmentService
{
    Task<ShipmentResponse> CreateAsync(CreateShipmentRequest request);
    Task<ShipmentResponse?> GetByTrackingCodeAsync(string trackingCode);
    Task<ShipmentResponse> AssignAsync(int id, AssignRequest request);
    Task<ShipmentResponse> UpdateStatusAsync(int id, UpdateStatusRequest request);
    Task<IEnumerable<ShipmentHistoryResponse>> GetHistoryAsync(int id);
}
