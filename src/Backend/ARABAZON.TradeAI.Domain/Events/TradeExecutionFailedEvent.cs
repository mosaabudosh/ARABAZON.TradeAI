using ARABAZON.TradeAI.Domain.Common;

namespace ARABAZON.TradeAI.Domain.Events;

public sealed class TradeExecutionFailedEvent : BaseDomainEvent
{
    public Guid TradeId { get; }
    public string FailureReason { get; }

    public TradeExecutionFailedEvent(Guid tradeId, string failureReason)
    {
        TradeId = tradeId;
        FailureReason = failureReason;
    }
}