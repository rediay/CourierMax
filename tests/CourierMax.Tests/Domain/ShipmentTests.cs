using CourierMax.Domain.Entities;
using CourierMax.Domain.Enums;
using CourierMax.Domain.ValueObjects;
using FluentAssertions;

namespace CourierMax.Tests.Domain.Entities;

public class ShipmentTests
{
    [Fact]
    public void Create_ValidData_SetsProperties()
    {
        var senderName = "Juan Perez";
        var senderPhone = new Phone("3001234567");
        var senderAddress = new Address("Calle 123");
        var recipientName = "Maria Lopez";
        var recipientPhone = new Phone("3102345678");
        var recipientAddress = new Address("Carrera 456");
        var weight = new Weight(5);
        var dimensions = new Dimensions(30, 20, 10);
        var packageType = PackageType.Paquete;
        var serviceType = ServiceType.Estandar;
        var origin = "Bogotá";
        var destination = "Medellín";

        var shipment = new Shipment(
            senderName, senderPhone, senderAddress,
            recipientName, recipientPhone, recipientAddress,
            weight, dimensions, packageType, serviceType,
            origin, destination);

        shipment.TrackingCode.ToString().Should().MatchRegex(@"^CM-\d{8}$");
        shipment.Status.Should().Be(ShipmentStatus.CREADO);
        shipment.Origin.Should().Be("Bogotá");
        shipment.Destination.Should().Be("Medellín");
        shipment.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Assign_ValidData_SetsPropertiesAndTransitionsToAsignado()
    {
        var shipment = CreateValidShipment();
        var vehicleId = 1;
        var driverId = 2;

        shipment.Assign(vehicleId, driverId, "operator", 12500);

        shipment.VehicleId.Should().Be(vehicleId);
        shipment.DriverId.Should().Be(driverId);
        shipment.Status.Should().Be(ShipmentStatus.ASIGNADO);
        shipment.StatusHistories.Should().Contain(h =>
            h.PreviousStatus == ShipmentStatus.CREADO &&
            h.NewStatus == ShipmentStatus.ASIGNADO &&
            h.ChangedBy == "operator");
    }

    [Fact]
    public void Assign_NotInCreado_ThrowsInvalidOperationException()
    {
        var shipment = CreateValidShipment();
        shipment.Assign(1, 2, "operator", 0);

        Action act = () => shipment.Assign(3, 4, "operator", 0);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot assign*ASIGNADO*CREADO*");
    }

    [Fact]
    public void Assign_WithTotalCost_SetsTotalCost()
    {
        var shipment = CreateValidShipment();

        shipment.Assign(1, 2, "operator", 15000);

        shipment.TotalCost.Should().Be(15000);
    }

    private static Shipment CreateValidShipment()
    {
        return new Shipment(
            "Juan Perez", new Phone("3001234567"), new Address("Calle 123"),
            "Maria Lopez", new Phone("3102345678"), new Address("Carrera 456"),
            new Weight(5), new Dimensions(30, 20, 10),
            PackageType.Paquete, ServiceType.Estandar, "Bogotá", "Medellín");
    }
}
