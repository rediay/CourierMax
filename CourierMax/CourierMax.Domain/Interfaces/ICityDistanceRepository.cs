using CourierMax.Domain.Entities;

namespace CourierMax.Domain.Interfaces;

public interface ICityDistanceRepository
{
    Task<CityDistance?> GetByRouteAsync(string origin, string destination);
    Task<IEnumerable<CityDistance>> GetAllAsync();
}
