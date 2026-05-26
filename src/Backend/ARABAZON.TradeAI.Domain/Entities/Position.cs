using ARABAZON.TradeAI.Domain.ValueObjects;

namespace ARABAZON.TradeAI.Domain.Entities;

public class Position
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid TradeId { get; private set; }
    public Price CurrentPrice { get; private set; } = default!;
    public Money UnrealizedPnL { get; private set; } = Money.Zero();
    public decimal Exposure { get; private set; }
    public DateTime LastUpdated { get; private set; }

    private Position() { }

    public static Position Create(Guid tradeId, decimal currentPrice, decimal exposure) => new()
    {
        TradeId = tradeId,
        CurrentPrice = new Price(currentPrice),
        Exposure = exposure,
        LastUpdated = DateTime.UtcNow,
    };

    public void Update(decimal currentPrice, decimal unrealizedPnL)
    {
        CurrentPrice = new Price(currentPrice);
        UnrealizedPnL = new Money(unrealizedPnL);
        LastUpdated = DateTime.UtcNow;
    }
}