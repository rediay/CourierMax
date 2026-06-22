namespace CourierMax.Domain.ValueObjects;

public record Weight
{
    public decimal Kg { get; }

    public Weight(decimal kg)
    {
        if (kg < 0.1m || kg > 100m)
            throw new ArgumentException("Weight must be between 0.1 and 100 kg");

        Kg = kg;
    }

    public override string ToString() => $"{Kg:F2} kg";
}
