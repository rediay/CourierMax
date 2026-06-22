using CourierMax.Domain.Enums;
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
        string destination)
    {
        TrackingCode = TrackingCode.Generate();
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
    }
}
