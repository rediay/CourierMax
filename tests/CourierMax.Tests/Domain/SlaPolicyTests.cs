using FluentAssertions;
using CourierMax.Domain.Entities;
using CourierMax.Domain.Enums;
using CourierMax.Domain.Services;
using CourierMax.Domain.ValueObjects;

namespace CourierMax.Tests.Domain;

public class SlaPolicyTests
{
    private static Shipment CreateShipment(ServiceType serviceType, DateTime createdAt)
    {
        var shipment = new Shipment(
            "Juan Perez", new Phone("3001234567"), new Address("Calle 123"),
            "Maria Lopez", new Phone("3102345678"), new Address("Carrera 456"),
            new Weight(5), new Dimensions(30, 20, 10),
            PackageType.Paquete, serviceType, "Bogotá", "Medellín");

        typeof(Shipment).GetProperty(nameof(Shipment.CreatedAt))!.SetValue(shipment, createdAt);
        return shipment;
    }

    [Fact]
    public void IsOverdue_MismoDiaNotDeliveredNextDay_ReturnsTrue()
    {
        var shipment = CreateShipment(ServiceType.MismoDia, new DateTime(2026, 6, 22)); // Monday

        var isOverdue = SlaPolicy.IsOverdue(shipment, new DateTime(2026, 6, 23)); // Tuesday

        isOverdue.Should().BeTrue();
    }

    [Fact]
    public void IsOverdue_MismoDiaSameDay_ReturnsFalse()
    {
        var shipment = CreateShipment(ServiceType.MismoDia, new DateTime(2026, 6, 22));

        var isOverdue = SlaPolicy.IsOverdue(shipment, new DateTime(2026, 6, 22, 18, 0, 0));

        isOverdue.Should().BeFalse();
    }

    [Fact]
    public void IsOverdue_EstandarWithinFiveBusinessDays_ReturnsFalse()
    {
        var shipment = CreateShipment(ServiceType.Estandar, new DateTime(2026, 6, 22)); // Monday

        // 5 business days later -> Monday 2026-06-29 is a holiday, so day 5 lands on Tue 2026-06-30
        var withinSla = SlaPolicy.IsOverdue(shipment, new DateTime(2026, 6, 30));

        withinSla.Should().BeFalse();
    }

    [Fact]
    public void IsOverdue_EstandarBeyondFiveBusinessDays_ReturnsTrue()
    {
        var shipment = CreateShipment(ServiceType.Estandar, new DateTime(2026, 6, 22));

        var overdue = SlaPolicy.IsOverdue(shipment, new DateTime(2026, 7, 1));

        overdue.Should().BeTrue();
    }

    [Fact]
    public void IsOverdue_CancelledShipment_NeverOverdue()
    {
        var shipment = CreateShipment(ServiceType.MismoDia, new DateTime(2026, 1, 1));
        shipment.Cancel("Cliente solicitó cancelación", "operator");

        var overdue = SlaPolicy.IsOverdue(shipment, new DateTime(2026, 6, 22));

        overdue.Should().BeFalse();
    }
}
