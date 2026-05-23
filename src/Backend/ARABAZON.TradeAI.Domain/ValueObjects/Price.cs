namespace ARABAZON.TradeAI.Domain.ValueObjects;

public sealed record Price
{
    public decimal Value { get; }

    public Price(decimal value)
    {
        if (value < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(value));
        Value = Math.Round(value, 5);
    }

    public static Price Zero => new(0);
    public static implicit operator decimal(Price price) => price.Value;
    public override string ToString() => Value.ToString("F5");
}