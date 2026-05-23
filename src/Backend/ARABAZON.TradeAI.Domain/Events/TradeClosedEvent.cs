using ARABAZON.TradeAI.Domain.Common;

namespace ARABAZON.TradeAI.Domain.Events;

public sealed class TradeClosedEvent : BaseDomainEvent
{
    public Guid TradeId { get; }
    public decimal ProfitLoss { get; }
    public DateTime ClosedAt { get; }

    public TradeClosedEvent(Guid tradeId, decimal profitLoss, DateTime closedAt)
    {
        TradeId = tradeId;
        ProfitLoss = profitLoss;
        ClosedAt = closedAt;
    }
}