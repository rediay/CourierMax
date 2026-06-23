namespace CourierMax.Application.DTOs;

public class DriverEfficiencyReportResponse
{
    public int DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public int TotalAssigned { get; set; }
    public int TotalDelivered { get; set; }
    public int TotalCancelled { get; set; }
    public int TotalInTransit { get; set; }
    public double AverageDeliveryDays { get; set; }
    public double OnTimeDeliveryPercentage { get; set; }
    public decimal TotalWeightTransportedKg { get; set; }
}
