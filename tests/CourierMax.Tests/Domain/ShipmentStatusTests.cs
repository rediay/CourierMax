using CourierMax.Domain.Entities;
using CourierMax.Domain.Enums;
using CourierMax.Domain.Exceptions;
using CourierMax.Domain.ValueObjects;
using FluentAssertions;

namespace CourierMax.Tests.Domain.Entities;

public class ShipmentStatusTests
{
    private static Shipment CreateValidShipment()
    {
        return new Shipment(
            "Juan Perez", new Phone("3001234567"), new Address("Calle 123"),
            "Maria Lopez", new Phone("3102345678"), new Address("Carrera 456"),
            new Weight(5), new Dimensions(30, 20, 10),
            PackageType.Paquete, ServiceType.Estandar, "Bogotá", "Medellín");
    }

    [Fact]
    public void Create_SetsStatusToCREADO()
    {
        var shipment = CreateValidShipment();
        shipment.Status.Should().Be(ShipmentStatus.CREADO);
    }

    [Fact]
    public void Assign_FromCREADO_SetsASIGNADO()
    {
        var shipment = CreateValidShipment();
        shipment.Assign(1, 1, "operator1", 0);
        shipment.Status.Should().Be(ShipmentStatus.ASIGNADO);
        shipment.VehicleId.Should().Be(1);
        shipment.DriverId.Should().Be(1);
    }

    [Fact]
    public void Assign_FromNonCREADO_Throws()
    {
        var shipment = CreateValidShipment();
        shipment.Assign(1, 1, "operator1", 0);
        Action act = () => shipment.Assign(1, 1, "operator2", 0);
        act.Should().Throw<ShipmentStateConflictException>();
    }

    [Fact]
    public void MarkInTransit_FromASIGNADO_SetsEN_TRANSITO()
    {
        var shipment = CreateValidShipment();
        shipment.Assign(1, 1, "op1", 0);
        shipment.MarkInTransit("op1");
        shipment.Status.Should().Be(ShipmentStatus.EN_TRANSITO);
    }

    [Fact]
    public void MarkInTransit_FromCREADO_Throws()
    {
        var shipment = CreateValidShipment();
        Action act = () => shipment.MarkInTransit("op1");
        act.Should().Throw<ShipmentStateConflictException>();
    }

    [Fact]
    public void Deliver_FromEN_TRANSITO_SetsENTREGADO()
    {
        var shipment = CreateValidShipment();
        shipment.Assign(1, 1, "op1", 0);
        shipment.MarkInTransit("op1");
        shipment.Deliver("op1");
        shipment.Status.Should().Be(ShipmentStatus.ENTREGADO);
    }

    [Fact]
    public void Deliver_FromCREADO_Throws()
    {
        var shipment = CreateValidShipment();
        Action act = () => shipment.Deliver("op1");
        act.Should().Throw<ShipmentStateConflictException>();
    }

    [Fact]
    public void Cancel_FromCREADO_SetsCANCELADO()
    {
        var shipment = CreateValidShipment();
        shipment.Cancel("Package lost in warehouse", "op1");
        shipment.Status.Should().Be(ShipmentStatus.CANCELADO);
    }

    [Fact]
    public void Cancel_FromASIGNADO_SetsCANCELADO()
    {
        var shipment = CreateValidShipment();
        shipment.Assign(1, 1, "op1", 0);
        shipment.Cancel("Client requested cancellation", "op1");
        shipment.Status.Should().Be(ShipmentStatus.CANCELADO);
    }

    [Fact]
    public void Cancel_FromEN_TRANSITO_SetsCANCELADO()
    {
        var shipment = CreateValidShipment();
        shipment.Assign(1, 1, "op1", 0);
        shipment.MarkInTransit("op1");
        shipment.Cancel("Vehicle breakdown", "op1");
        shipment.Status.Should().Be(ShipmentStatus.CANCELADO);
    }

    [Fact]
    public void Cancel_FromENTREGADO_Throws()
    {
        var shipment = CreateValidShipment();
        shipment.Assign(1, 1, "op1", 0);
        shipment.MarkInTransit("op1");
        shipment.Deliver("op1");
        Action act = () => shipment.Cancel("Too late", "op1");
        act.Should().Throw<ShipmentStateConflictException>();
    }

    [Fact]
    public void Cancel_WithoutReason_Throws()
    {
        var shipment = CreateValidShipment();
        Action act = () => shipment.Cancel("", "op1");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Cancel_WithShortReason_Throws()
    {
        var shipment = CreateValidShipment();
        Action act = () => shipment.Cancel("abc", "op1");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FullFlow_CREADO_to_ENTREGADO()
    {
        var shipment = CreateValidShipment();
        shipment.Status.Should().Be(ShipmentStatus.CREADO);
        shipment.Assign(1, 1, "op1", 0);
        shipment.Status.Should().Be(ShipmentStatus.ASIGNADO);
        shipment.MarkInTransit("op1");
        shipment.Status.Should().Be(ShipmentStatus.EN_TRANSITO);
        shipment.Deliver("op1");
        shipment.Status.Should().Be(ShipmentStatus.ENTREGADO);
    }

    [Fact]
    public void StatusHistory_RecordsAllTransitions()
    {
        var shipment = CreateValidShipment();
        shipment.Assign(1, 1, "op1", 0);
        shipment.MarkInTransit("op1");
        shipment.Deliver("op1");

        shipment.StatusHistories.Should().HaveCount(4);
        shipment.StatusHistories.ElementAt(0).NewStatus.Should().Be(ShipmentStatus.CREADO);
        shipment.StatusHistories.ElementAt(1).NewStatus.Should().Be(ShipmentStatus.ASIGNADO);
        shipment.StatusHistories.ElementAt(2).NewStatus.Should().Be(ShipmentStatus.EN_TRANSITO);
        shipment.StatusHistories.ElementAt(3).NewStatus.Should().Be(ShipmentStatus.ENTREGADO);
    }
}
