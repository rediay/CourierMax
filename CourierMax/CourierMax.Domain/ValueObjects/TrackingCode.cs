namespace CourierMax.Domain.ValueObjects;

public record TrackingCode
{
    public string Value { get; }

    private TrackingCode(string value)
    {
        Value = value;
    }

    public static TrackingCode Generate()
    {
        var random = new Random();
        var digits = random.Next(0, 100_000_000).ToString("D8");
        return new TrackingCode($"CM-{digits}");
    }

    public static TrackingCode FromString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Tracking code cannot be empty");

        if (!System.Text.RegularExpressions.Regex.IsMatch(value, @"^CM-\d{8}$"))
            throw new ArgumentException("Tracking code must be in format CM-XXXXXXXX (8 digits)");

        return new TrackingCode(value);
    }

    public override string ToString() => Value;
}
