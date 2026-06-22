using CourierMax.Domain.Enums;

namespace CourierMax.Domain.Entities;

public class ShipmentStatusHistory
{
    public int Id { get; private set; }
    public int ShipmentId { get; private set; }
    public ShipmentStatus? PreviousStatus { get; private set; }
    public ShipmentStatus NewStatus { get; private set; }
    public DateTime ChangedAt { get; private set; }
    public string? Reason { get; private set; }
    public string ChangedBy { get; private set; }

    private ShipmentStatusHistory()
    {
        ChangedBy = null!;
    }

    public ShipmentStatusHistory(
        int shipmentId,
        ShipmentStatus? previousStatus,
        ShipmentStatus newStatus,
        string changedBy,
        string? reason = null)
    {
        ShipmentId = shipmentId;
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        ChangedBy = changedBy;
        Reason = reason;
        ChangedAt = DateTime.UtcNow;
    }
}
