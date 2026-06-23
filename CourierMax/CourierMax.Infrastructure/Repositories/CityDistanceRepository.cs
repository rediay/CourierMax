using Microsoft.EntityFrameworkCore;
using CourierMax.Domain.Entities;
using CourierMax.Domain.Interfaces;

namespace CourierMax.Infrastructure.Repositories;

public class CityDistanceRepository : ICityDistanceRepository
{
    private readonly Data.CourierMaxDbContext _context;

    public CityDistanceRepository(Data.CourierMaxDbContext context)
    {
        _context = context;
    }

    public async Task<CityDistance?> GetByRouteAsync(string origin, string destination)
    {
        return await _context.CityDistances
            .FirstOrDefaultAsync(c =>
                (c.Origin == origin && c.Destination == destination) ||
                (c.Origin == destination && c.Destination == origin));
    }

    public async Task<IEnumerable<CityDistance>> GetAllAsync()
    {
        return await _context.CityDistances.ToListAsync();
    }
}
