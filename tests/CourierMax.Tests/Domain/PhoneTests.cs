using CourierMax.Domain.ValueObjects;
using FluentAssertions;

namespace CourierMax.Tests.Domain.ValueObjects;

public class PhoneTests
{
    [Theory]
    [InlineData("3001234567")]
    [InlineData("3102345678")]
    [InlineData("6012345678")]
    [InlineData("320 345 6789")]
    [InlineData("320-345-6789")]
    public void Create_ValidPhone_Success(string input)
    {
        var phone = new Phone(input);
        phone.Value.Should().HaveLength(10);
        phone.Value.Should().MatchRegex("^[36]");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("12345")]
    [InlineData("12345678901")]
    [InlineData("7001234567")]
    [InlineData("5001234567")]
    public void Create_InvalidPhone_Throws(string input)
    {
        Action act = () => new Phone(input);
        act.Should().Throw<ArgumentException>();
    }
}
