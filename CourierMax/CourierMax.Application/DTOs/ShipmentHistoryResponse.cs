namespace CourierMax.Application.DTOs;

public class ShipmentHistoryResponse
{
    public int Id { get; set; }
    public string? PreviousStatus { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
    public string? Reason { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
}
