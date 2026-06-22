namespace CourierMax.Domain.ValueObjects;

public record Address
{
    public string Value { get; }

    public Address(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Address cannot be empty");

        Value = value.Trim();
    }

    public override string ToString() => Value;
}
