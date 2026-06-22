namespace CourierMax.Domain.ValueObjects;

public record Dimensions
{
    public decimal LengthCm { get; }
    public decimal WidthCm { get; }
    public decimal HeightCm { get; }

    public Dimensions(decimal lengthCm, decimal widthCm, decimal heightCm)
    {
        if (lengthCm < 1 || lengthCm > 200)
            throw new ArgumentException("Length must be between 1 and 200 cm");
        if (widthCm < 1 || widthCm > 200)
            throw new ArgumentException("Width must be between 1 and 200 cm");
        if (heightCm < 1 || heightCm > 200)
            throw new ArgumentException("Height must be between 1 and 200 cm");

        LengthCm = lengthCm;
        WidthCm = widthCm;
        HeightCm = heightCm;
    }

    public decimal VolumeM3 => (LengthCm * WidthCm * HeightCm) / 1_000_000m;
}
