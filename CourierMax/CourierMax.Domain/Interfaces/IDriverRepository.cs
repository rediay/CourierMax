using CourierMax.Domain.Entities;

namespace CourierMax.Domain.Interfaces;

public interface IDriverRepository
{
    Task<Driver?> GetByIdAsync(int id);
    Task<IEnumerable<Driver>> GetAllAsync();
}
