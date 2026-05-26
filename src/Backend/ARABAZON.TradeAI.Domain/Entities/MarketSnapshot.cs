using ARABAZON.TradeAI.Domain.Enums;

namespace ARABAZON.TradeAI.Domain.Entities;

public class MarketSnapshot
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid SymbolId { get; private set; }
    public decimal BidPrice { get; private set; }
    public decimal AskPrice { get; private set; }
    public decimal Spread { get; private set; }
    public VolatilityLevel Volatility { get; private set; }
    public DateTime SnapshotTime { get; private set; }

    private MarketSnapshot() { }

    public static MarketSnapshot Create(Guid symbolId, decimal bid, decimal ask)
    {
        var spread = ask - bid;
        return new MarketSnapshot
        {
            SymbolId = symbolId,
            BidPrice = bid,
            AskPrice = ask,
            Spread = Math.Round(spread, 5),
            Volatility = VolatilityLevel.Low,
            SnapshotTime = DateTime.UtcNow,
        };
    }

    public decimal MidPrice => Math.Round((BidPrice + AskPrice) / 2, 5);
}