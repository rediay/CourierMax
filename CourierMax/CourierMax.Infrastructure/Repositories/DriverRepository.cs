using Microsoft.EntityFrameworkCore;
using CourierMax.Domain.Entities;
using CourierMax.Domain.Interfaces;

namespace CourierMax.Infrastructure.Repositories;

public class DriverRepository : IDriverRepository
{
    private readonly Data.CourierMaxDbContext _context;

    public DriverRepository(Data.CourierMaxDbContext context)
    {
        _context = context;
    }

    public async Task<Driver?> GetByIdAsync(int id)
    {
        return await _context.Drivers.FindAsync(id);
    }

    public async Task<IEnumerable<Driver>> GetAllAsync()
    {
        return await _context.Drivers.ToListAsync();
    }
}
