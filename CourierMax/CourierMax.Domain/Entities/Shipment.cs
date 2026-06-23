using CourierMax.Domain.Enums;
using CourierMax.Domain.Exceptions;
using CourierMax.Domain.ValueObjects;

namespace CourierMax.Domain.Entities;

public class Shipment
{
    public int Id { get; private set; }
    public TrackingCode TrackingCode { get; private set; }

    public string SenderName { get; private set; }
    public Phone SenderPhone { get; private set; }
    public Address SenderAddress { get; private set; }

    public string RecipientName { get; private set; }
    public Phone RecipientPhone { get; private set; }
    public Address RecipientAddress { get; private set; }

    public Weight PackageWeight { get; private set; }
    public Dimensions PackageDimensions { get; private set; }
    public PackageType PackageType { get; private set; }

    public ServiceType ServiceType { get; private set; }
    public string Origin { get; private set; }
    public string Destination { get; private set; }

    public ShipmentStatus Status { get; private set; }
    public int? VehicleId { get; private set; }
    public int? DriverId { get; private set; }
    public decimal? TotalCost { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private readonly List<ShipmentStatusHistory> _statusHistories = new();
    public IReadOnlyCollection<ShipmentStatusHistory> StatusHistories => _statusHistories;

    private Shipment()
    {
        TrackingCode = null!;
        SenderName = null!;
        SenderPhone = null!;
        SenderAddress = null!;
        RecipientName = null!;
        RecipientPhone = null!;
        RecipientAddress = null!;
        PackageWeight = null!;
        PackageDimensions = null!;
        Origin = null!;
        Destination = null!;
    }

    public Shipment(
        string senderName,
        Phone senderPhone,
        Address senderAddress,
        string recipientName,
        Phone recipientPhone,
        Address recipientAddress,
        Weight packageWeight,
        Dimensions packageDimensions,
        PackageType packageType,
        ServiceType serviceType,
        string origin,
        string destination,
        TrackingCode? trackingCode = null)
    {
        TrackingCode = trackingCode ?? TrackingCode.Generate();
        SenderName = senderName;
        SenderPhone = senderPhone;
        SenderAddress = senderAddress;
        RecipientName = recipientName;
        RecipientPhone = recipientPhone;
        RecipientAddress = recipientAddress;
        PackageWeight = packageWeight;
        PackageDimensions = packageDimensions;
        PackageType = packageType;
        ServiceType = serviceType;
        Origin = origin;
        Destination = destination;
        Status = ShipmentStatus.CREADO;
        CreatedAt = DateTime.UtcNow;

        _statusHistories.Add(new ShipmentStatusHistory(0, null, ShipmentStatus.CREADO, "system"));
    }

    public void Assign(int vehicleId, int driverId, string changedBy, decimal totalCost)
    {
        if (Status != ShipmentStatus.CREADO)
            throw new ShipmentStateConflictException($"Cannot assign shipment in status {Status}. Must be {ShipmentStatus.CREADO}.");

        VehicleId = vehicleId;
        DriverId = driverId;
        TotalCost = totalCost;
        TransitionTo(ShipmentStatus.ASIGNADO, changedBy);
    }

    public void MarkInTransit(string changedBy)
    {
        if (Status != ShipmentStatus.ASIGNADO)
            throw new ShipmentStateConflictException($"Cannot mark as in transit from status {Status}. Must be {ShipmentStatus.ASIGNADO}.");

        TransitionTo(ShipmentStatus.EN_TRANSITO, changedBy);
    }

    public void Deliver(string changedBy)
    {
        if (Status != ShipmentStatus.EN_TRANSITO)
            throw new ShipmentStateConflictException($"Cannot deliver shipment in status {Status}. Must be {ShipmentStatus.EN_TRANSITO}.");

        TransitionTo(ShipmentStatus.ENTREGADO, changedBy);
    }

    public void Cancel(string reason, string changedBy)
    {
        if (Status == ShipmentStatus.ENTREGADO)
            throw new ShipmentStateConflictException("Cannot cancel a delivered shipment.");

        if (Status == ShipmentStatus.CANCELADO)
            throw new ShipmentStateConflictException("Shipment is already cancelled.");

        if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length < 5)
            throw new ArgumentException("Cancellation reason must be at least 5 characters.");

        TransitionTo(ShipmentStatus.CANCELADO, changedBy, reason.Trim());
    }

    private void TransitionTo(ShipmentStatus newStatus, string changedBy, string? reason = null)
    {
        var previous = Status;
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;

        _statusHistories.Add(new ShipmentStatusHistory(Id, previous, newStatus, changedBy, reason));
    }
}
