using FluentAssertions;
using CourierMax.Domain.Entities;

namespace CourierMax.Tests.Domain;

public class VehicleTests
{
    [Fact]
    public void HasCapacityFor_WithinLimits_ReturnsTrue()
    {
        var vehicle = new Vehicle("ABC-123", 1, 500, 10);

        vehicle.HasCapacityFor(100, 2).Should().BeTrue();
    }

    [Fact]
    public void HasCapacityFor_ExceedsWeight_ReturnsFalse()
    {
        var vehicle = new Vehicle("ABC-123", 1, 500, 10);

        vehicle.HasCapacityFor(600, 2).Should().BeFalse();
    }

    [Fact]
    public void HasCapacityFor_ExceedsVolume_ReturnsFalse()
    {
        var vehicle = new Vehicle("ABC-123", 1, 500, 10);

        vehicle.HasCapacityFor(100, 11).Should().BeFalse();
    }

    [Fact]
    public void LoadCargo_WithinCapacity_IncreasesCurrentLoad()
    {
        var vehicle = new Vehicle("ABC-123", 1, 500, 10);

        vehicle.LoadCargo(100, 2);

        vehicle.CurrentWeightKg.Should().Be(100);
        vehicle.CurrentVolumeM3.Should().Be(2);
    }

    [Fact]
    public void LoadCargo_ExceedingCapacity_ThrowsInvalidOperationException()
    {
        var vehicle = new Vehicle("ABC-123", 1, 500, 10);
        vehicle.LoadCargo(450, 9);

        Action act = () => vehicle.LoadCargo(100, 1);

        act.Should().Throw<InvalidOperationException>().WithMessage("*capacity*");
    }

    [Fact]
    public void ReleaseCargo_AfterLoad_RestoresCapacity()
    {
        var vehicle = new Vehicle("ABC-123", 1, 500, 10);
        vehicle.LoadCargo(100, 2);

        vehicle.ReleaseCargo(100, 2);

        vehicle.CurrentWeightKg.Should().Be(0);
        vehicle.CurrentVolumeM3.Should().Be(0);
    }

    [Fact]
    public void ReleaseCargo_MoreThanLoaded_ClampsToZero()
    {
        var vehicle = new Vehicle("ABC-123", 1, 500, 10);
        vehicle.LoadCargo(50, 1);

        vehicle.ReleaseCargo(100, 5);

        vehicle.CurrentWeightKg.Should().Be(0);
        vehicle.CurrentVolumeM3.Should().Be(0);
    }
}
