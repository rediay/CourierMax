using Microsoft.EntityFrameworkCore;
using CourierMax.Domain.Entities;
using CourierMax.Domain.Interfaces;

namespace CourierMax.Infrastructure.Repositories;

public class ShipmentRepository : IShipmentRepository
{
    private readonly Data.CourierMaxDbContext _context;

    public ShipmentRepository(Data.CourierMaxDbContext context)
    {
        _context = context;
    }

    public async Task<Shipment?> GetByIdAsync(int id)
    {
        return await _context.Shipments
            .Include(s => s.StatusHistories)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Shipment?> GetByTrackingCodeAsync(string trackingCode)
    {
        return await _context.Shipments
            .Include(s => s.StatusHistories)
            .FirstOrDefaultAsync(s => s.TrackingCode.Value == trackingCode);
    }

    public async Task<IEnumerable<Shipment>> GetAllAsync()
    {
        return await _context.Shipments
            .Include(s => s.StatusHistories)
            .ToListAsync();
    }

    public async Task AddAsync(Shipment shipment)
    {
        await _context.Shipments.AddAsync(shipment);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Shipment shipment)
    {
        _context.Shipments.Update(shipment);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> TrackingCodeExistsAsync(string trackingCode)
    {
        return await _context.Shipments
            .AnyAsync(s => s.TrackingCode.Value == trackingCode);
    }
}
