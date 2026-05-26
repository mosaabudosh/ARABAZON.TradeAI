namespace ARABAZON.TradeAI.Domain.Entities;

public class Indicator
{
    public long Id { get; private set; }
    public long CandleId { get; private set; }
    public string IndicatorType { get; private set; } = default!;
    public decimal Value { get; private set; }
    public DateTime CalculatedAt { get; private set; }

    private Indicator() { }

    public static Indicator Create(long candleId, string indicatorType, decimal value) => new()
    {
        CandleId = candleId,
        IndicatorType = indicatorType,
        Value = Math.Round(value, 8),
        CalculatedAt = DateTime.UtcNow,
    };
}