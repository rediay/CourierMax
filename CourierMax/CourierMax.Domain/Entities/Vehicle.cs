namespace CourierMax.Domain.Entities;

public class Vehicle
{
    public int Id { get; private set; }
    public string Plate { get; private set; }
    public int? DriverId { get; private set; }
    public decimal MaxWeightKg { get; private set; }
    public decimal MaxVolumeM3 { get; private set; }
    public decimal CurrentWeightKg { get; private set; }
    public decimal CurrentVolumeM3 { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public Driver? Driver { get; private set; }

    private Vehicle()
    {
        Plate = null!;
    }

    public Vehicle(string plate, int? driverId, decimal maxWeightKg, decimal maxVolumeM3)
    {
        Plate = plate;
        DriverId = driverId;
        MaxWeightKg = maxWeightKg;
        MaxVolumeM3 = maxVolumeM3;
        CurrentWeightKg = 0;
        CurrentVolumeM3 = 0;
        CreatedAt = DateTime.UtcNow;
    }

    public bool HasCapacityFor(decimal weightKg, decimal volumeM3) =>
        CurrentWeightKg + weightKg <= MaxWeightKg &&
        CurrentVolumeM3 + volumeM3 <= MaxVolumeM3;

    public void LoadCargo(decimal weightKg, decimal volumeM3)
    {
        if (!HasCapacityFor(weightKg, volumeM3))
            throw new InvalidOperationException(
                $"Vehicle {Plate} does not have enough capacity. " +
                $"Available weight: {MaxWeightKg - CurrentWeightKg}kg, available volume: {MaxVolumeM3 - CurrentVolumeM3}m3.");

        CurrentWeightKg += weightKg;
        CurrentVolumeM3 += volumeM3;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReleaseCargo(decimal weightKg, decimal volumeM3)
    {
        CurrentWeightKg = Math.Max(0, CurrentWeightKg - weightKg);
        CurrentVolumeM3 = Math.Max(0, CurrentVolumeM3 - volumeM3);
        UpdatedAt = DateTime.UtcNow;
    }
}
