using ARABAZON.TradeAI.Domain.Common;
using ARABAZON.TradeAI.Domain.Enums;

namespace ARABAZON.TradeAI.Domain.Events;

public sealed class SignalGeneratedEvent : BaseDomainEvent
{
    public Guid SignalId { get; }
    public Guid SymbolId { get; }
    public SignalType SignalType { get; }
    public decimal ConfidenceScore { get; }

    public SignalGeneratedEvent(Guid signalId, Guid symbolId, SignalType signalType, decimal confidenceScore)
    {
        SignalId = signalId;
        SymbolId = symbolId;
        SignalType = signalType;
        ConfidenceScore = confidenceScore;
    }
}