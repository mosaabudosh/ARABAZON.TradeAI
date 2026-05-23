using ARABAZON.TradeAI.Domain.Common;
using ARABAZON.TradeAI.Domain.Enums;
using ARABAZON.TradeAI.Domain.Events;
using ARABAZON.TradeAI.Domain.ValueObjects;

namespace ARABAZON.TradeAI.Domain.Entities;

public class TradeSignal : BaseEntity
{
    public Guid SymbolId { get; private set; }
    public SignalType SignalType { get; private set; }
    public string StrategyName { get; private set; } = default!;
    public Price EntryPrice { get; private set; } = default!;
    public Price StopLoss { get; private set; } = default!;
    public Price TakeProfit { get; private set; } = default!;
    public Percentage ConfidenceScore { get; private set; } = default!;
    public Percentage RiskPercentage { get; private set; } = default!;
    public SignalStatus Status { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime GeneratedAt { get; private set; }

    private TradeSignal() { }

    public static TradeSignal Create(Guid symbolId, SignalType signalType, string strategyName,
        decimal entryPrice, decimal stopLoss, decimal takeProfit,
        decimal confidenceScore, decimal riskPercentage, TimeSpan? validity = null)
    {
        var signal = new TradeSignal
        {
            SymbolId = symbolId,
            SignalType = signalType,
            StrategyName = strategyName,
            EntryPrice = new Price(entryPrice),
            StopLoss = new Price(stopLoss),
            TakeProfit = new Price(takeProfit),
            ConfidenceScore = new Percentage(confidenceScore),
            RiskPercentage = new Percentage(riskPercentage),
            Status = SignalStatus.Pending,
            GeneratedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(validity ?? TimeSpan.FromHours(4))
        };

        signal.AddDomainEvent(new SignalGeneratedEvent(signal.Id, symbolId, signalType, confidenceScore));
        return signal;
    }

    public void Approve()
    {
        if (Status != SignalStatus.Pending)
            throw new InvalidOperationException($"Cannot approve signal in {Status} status.");
        if (DateTime.UtcNow > ExpiresAt)
            throw new InvalidOperationException("Cannot approve expired signal.");

        Status = SignalStatus.Approved;
        SetUpdatedAt();
        AddDomainEvent(new SignalApprovedEvent(Id));
    }

    public void Reject(string reason)
    {
        if (Status != SignalStatus.Pending)
            throw new InvalidOperationException($"Cannot reject signal in {Status} status.");

        Status = SignalStatus.Rejected;
        SetUpdatedAt();
    }

    public void MarkAsExecuted()
    {
        if (Status != SignalStatus.Approved)
            throw new InvalidOperationException("Only approved signals can be executed.");

        Status = SignalStatus.Executed;
        SetUpdatedAt();
    }

    public void Expire()
    {
        if (Status == SignalStatus.Executed || Status == SignalStatus.Rejected) return;
        Status = SignalStatus.Expired;
        SetUpdatedAt();
    }

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
}