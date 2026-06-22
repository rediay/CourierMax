using CourierMax.Domain.Entities;

namespace CourierMax.Domain.Interfaces;

public interface IShipmentRepository
{
    Task<Shipment?> GetByIdAsync(int id);
    Task<Shipment?> GetByTrackingCodeAsync(string trackingCode);
    Task<IEnumerable<Shipment>> GetAllAsync();
    Task AddAsync(Shipment shipment);
    Task UpdateAsync(Shipment shipment);
    Task<bool> TrackingCodeExistsAsync(string trackingCode);
}
