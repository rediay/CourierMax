using CourierMax.Domain.Entities;

namespace CourierMax.Domain.Interfaces;

public interface IVehicleRepository
{
    Task<Vehicle?> GetByIdAsync(int id);
    Task<IEnumerable<Vehicle>> GetAllAsync();
    Task<Vehicle?> GetByDriverIdAsync(int driverId);
    Task<IEnumerable<Vehicle>> GetAllWithActiveDriverAsync();
    Task UpdateAsync(Vehicle vehicle);
}
