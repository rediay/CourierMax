namespace CourierMax.Application.DTOs;

public class CostEstimateResponse
{
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }
    public decimal BaseFee { get; set; }
    public decimal WeightSurcharge { get; set; }
    public decimal DistanceFee { get; set; }
    public decimal PackageSurcharge { get; set; }
    public decimal TotalCost { get; set; }
}
