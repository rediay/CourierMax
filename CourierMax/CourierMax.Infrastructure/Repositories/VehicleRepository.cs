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
}
