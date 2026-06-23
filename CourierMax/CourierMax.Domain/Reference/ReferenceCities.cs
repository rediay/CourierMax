namespace CourierMax.Domain.Reference;

public static class ReferenceCities
{
    public static readonly IReadOnlySet<string> ValidCities = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Bogotá",
        "Medellín",
        "Cali",
        "Barranquilla"
    };

    public static bool IsValid(string city) => ValidCities.Contains(city);
}
