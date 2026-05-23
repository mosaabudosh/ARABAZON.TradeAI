namespace ARABAZON.TradeAI.Domain.ValueObjects;

public sealed record Percentage
{
    public decimal Value { get; }

    public Percentage(decimal value)
    {
        if (value < 0 || value > 100)
            throw new ArgumentException("Percentage must be between 0 and 100.", nameof(value));
        Value = Math.Round(value, 4);
    }

    public static Percentage Zero => new(0);
    public static implicit operator decimal(Percentage p) => p.Value;
    public override string ToString() => $"{Value:F2}%";
}