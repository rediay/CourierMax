namespace CourierMax.Domain.Entities;

public class CityDistance
{
    public int Id { get; private set; }
    public string Origin { get; private set; }
    public string Destination { get; private set; }
    public decimal DistanceKm { get; private set; }
    public decimal DistanceFee { get; private set; }

    private CityDistance()
    {
        Origin = null!;
        Destination = null!;
    }

    public CityDistance(string origin, string destination, decimal distanceKm, decimal distanceFee)
    {
        Origin = origin;
        Destination = destination;
        DistanceKm = distanceKm;
        DistanceFee = distanceFee;
    }
}
