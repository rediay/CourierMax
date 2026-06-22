using CourierMax.Domain.ValueObjects;
using FluentAssertions;

namespace CourierMax.Tests.Domain.ValueObjects;

public class DimensionsTests
{
    [Theory]
    [InlineData(10, 20, 30)]
    [InlineData(1, 1, 1)]
    [InlineData(200, 200, 200)]
    public void Create_ValidDimensions_Success(decimal l, decimal w, decimal h)
    {
        var dim = new Dimensions(l, w, h);
        dim.LengthCm.Should().Be(l);
        dim.WidthCm.Should().Be(w);
        dim.HeightCm.Should().Be(h);
    }

    [Theory]
    [InlineData(0, 10, 10)]
    [InlineData(10, 0, 10)]
    [InlineData(10, 10, 0)]
    [InlineData(201, 10, 10)]
    [InlineData(10, 201, 10)]
    [InlineData(10, 10, 201)]
    public void Create_InvalidDimensions_Throws(decimal l, decimal w, decimal h)
    {
        Action act = () => new Dimensions(l, w, h);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void VolumeM3_CalculatedCorrectly()
    {
        var dim = new Dimensions(100, 50, 20);
        dim.VolumeM3.Should().Be(0.1m);
    }
}
