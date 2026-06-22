using CourierMax.Domain.ValueObjects;
using FluentAssertions;

namespace CourierMax.Tests.Domain.ValueObjects;

public class TrackingCodeTests
{
    [Fact]
    public void Generate_ValidFormat()
    {
        var code = TrackingCode.Generate();
        code.ToString().Should().MatchRegex(@"^CM-\d{8}$");
    }

    [Fact]
    public void Generate_UniqueCodes()
    {
        var codes = Enumerable.Range(0, 100).Select(_ => TrackingCode.Generate().ToString()).ToHashSet();
        codes.Should().HaveCount(100);
    }

    [Fact]
    public void FromString_ValidCode_Success()
    {
        var code = TrackingCode.FromString("CM-12345678");
        code.ToString().Should().Be("CM-12345678");
    }

    [Theory]
    [InlineData("")]
    [InlineData("CM-1234")]
    [InlineData("CM-123456789")]
    [InlineData("XM-12345678")]
    public void FromString_InvalidCode_Throws(string input)
    {
        Action act = () => TrackingCode.FromString(input);
        act.Should().Throw<ArgumentException>();
    }
}
