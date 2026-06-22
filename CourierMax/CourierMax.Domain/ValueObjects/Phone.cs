namespace CourierMax.Domain.ValueObjects;

public record Phone
{
    public string Value { get; }

    public Phone(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Phone number cannot be empty");

        var digits = new string(value.Where(char.IsDigit).ToArray());

        if (digits.Length != 10)
            throw new ArgumentException("Phone number must have 10 digits (Colombian format)");

        if (digits[0] != '3' && digits[0] != '6')
            throw new ArgumentException("Phone number must start with 3 or 6 (Colombian format)");

        Value = digits;
    }

    public override string ToString() => Value;
}
