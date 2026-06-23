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
}
