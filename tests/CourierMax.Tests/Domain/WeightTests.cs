using CourierMax.Domain.ValueObjects;
using FluentAssertions;

namespace CourierMax.Tests.Domain.ValueObjects;

public class WeightTests
{
    [Theory]
    [InlineData(0.1)]
    [InlineData(50)]
    [InlineData(100)]
    public void Create_ValidWeight_Success(decimal kg)
    {
        var weight = new Weight(kg);
        weight.Kg.Should().Be(kg);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(100.01)]
    [InlineData(200)]
    public void Create_InvalidWeight_Throws(decimal kg)
    {
        Action act = () => new Weight(kg);
        act.Should().Throw<ArgumentException>();
    }
}
