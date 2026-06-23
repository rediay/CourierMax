namespace CourierMax.Application.DTOs;

public class ShipmentResponse
{
    public int Id { get; set; }
    public string TrackingCode { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string SenderPhone { get; set; } = string.Empty;
    public string SenderAddress { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public string RecipientAddress { get; set; } = string.Empty;
    public decimal PackageWeight { get; set; }
    public decimal PackageLength { get; set; }
    public decimal PackageWidth { get; set; }
    public decimal PackageHeight { get; set; }
    public string PackageType { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? VehicleId { get; set; }
    public int? DriverId { get; set; }
    public decimal? TotalCost { get; set; }
    public DateTime CreatedAt { get; set; }
}
