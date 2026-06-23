using Microsoft.EntityFrameworkCore;
using CourierMax.Domain.Entities;
using CourierMax.Domain.Interfaces;

namespace CourierMax.Infrastructure.Repositories;

public class VehicleRepository : IVehicleRepository
{
    private readonly Data.CourierMaxDbContext _context;

    public VehicleRepository(Data.CourierMaxDbContext context)
    {
        _context = context;
    }

    public async Task<Vehicle?> GetByIdAsync(int id)
    {
        return await _context.Vehicles.FindAsync(id);
    }

    public async Task<IEnumerable<Vehicle>> GetAllAsync()
    {
        return await _context.Vehicles.ToListAsync();
    }

    public async Task<Vehicle?> GetByDriverIdAsync(int driverId)
    {
        return await _context.Vehicles.FirstOrDefaultAsync(v => v.DriverId == driverId);
    }

    public async Task<IEnumerable<Vehicle>> GetAllWithActiveDriverAsync()
    {
        return await _context.Vehicles
            .Include(v => v.Driver)
            .Where(v => v.Driver != null && v.Driver.IsActive)
            .ToListAsync();
    }

    public async Task UpdateAsync(Vehicle vehicle)
    {
        _context.Vehicles.Update(vehicle);
        await _context.SaveChangesAsync();
    }
}
