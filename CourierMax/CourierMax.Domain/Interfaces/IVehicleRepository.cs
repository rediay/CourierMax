using CourierMax.Domain.Entities;

namespace CourierMax.Domain.Interfaces;

public interface IVehicleRepository
{
    Task<Vehicle?> GetByIdAsync(int id);
    Task<IEnumerable<Vehicle>> GetAllAsync();
}
