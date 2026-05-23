using ARABAZON.TradeAI.Domain.Enums;
using ARABAZON.TradeAI.Domain.ValueObjects;

namespace ARABAZON.TradeAI.Domain.Entities;

public class Candle
{
    public long Id { get; private set; }
    public Guid SymbolId { get; private set; }
    public Timeframe Timeframe { get; private set; }
    public Price OpenPrice { get; private set; } = default!;
    public Price HighPrice { get; private set; } = default!;
    public Price LowPrice { get; private set; } = default!;
    public Price ClosePrice { get; private set; } = default!;
    public decimal Volume { get; private set; }
    public DateTime OpenTime { get; private set; }
    public DateTime CloseTime { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Candle() { }

    public static Candle Create(Guid symbolId, Timeframe timeframe,
        decimal open, decimal high, decimal low, decimal close,
        decimal volume, DateTime openTime, DateTime closeTime)
    {
        return new Candle
        {
            SymbolId = symbolId,
            Timeframe = timeframe,
            OpenPrice = new Price(open),
            HighPrice = new Price(high),
            LowPrice = new Price(low),
            ClosePrice = new Price(close),
            Volume = volume,
            OpenTime = openTime,
            CloseTime = closeTime,
            CreatedAt = DateTime.UtcNow
        };
    }

    public bool IsBullish => ClosePrice.Value > OpenPrice.Value;
    public bool IsBearish => ClosePrice.Value < OpenPrice.Value;
    public decimal Range => HighPrice.Value - LowPrice.Value;
}