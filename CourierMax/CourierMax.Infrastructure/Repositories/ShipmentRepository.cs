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
        var tc = CourierMax.Domain.ValueObjects.TrackingCode.FromString(trackingCode);
        return await _context.Shipments
            .Include(s => s.StatusHistories)
            .FirstOrDefaultAsync(s => s.TrackingCode == tc);
    }

    public async Task<IEnumerable<Shipment>> GetAllAsync()
    {
        return await _context.Shipments
            .Include(s => s.StatusHistories)
            .ToListAsync();
    }

    public async Task<IEnumerable<Shipment>> GetByDriverIdAsync(int driverId)
    {
        return await _context.Shipments
            .Include(s => s.StatusHistories)
            .Where(s => s.DriverId == driverId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Shipment>> GetByCreatedDateRangeAsync(DateTime from, DateTime to)
    {
        return await _context.Shipments
            .Include(s => s.StatusHistories)
            .Where(s => s.CreatedAt >= from && s.CreatedAt <= to)
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
        var tc = CourierMax.Domain.ValueObjects.TrackingCode.FromString(trackingCode);
        return await _context.Shipments
            .AnyAsync(s => s.TrackingCode == tc);
    }
}
