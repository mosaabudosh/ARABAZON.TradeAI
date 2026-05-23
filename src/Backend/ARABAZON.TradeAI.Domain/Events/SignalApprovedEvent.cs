using ARABAZON.TradeAI.Domain.Common;

namespace ARABAZON.TradeAI.Domain.Events;

public sealed class SignalApprovedEvent : BaseDomainEvent
{
    public Guid SignalId { get; }
    public SignalApprovedEvent(Guid signalId) => SignalId = signalId;
}