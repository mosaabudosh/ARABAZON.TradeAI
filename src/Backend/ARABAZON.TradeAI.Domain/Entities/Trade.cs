using ARABAZON.TradeAI.Domain.Common;
using ARABAZON.TradeAI.Domain.Enums;
using ARABAZON.TradeAI.Domain.Events;
using ARABAZON.TradeAI.Domain.ValueObjects;

namespace ARABAZON.TradeAI.Domain.Entities;

public class Trade : BaseEntity
{
    public Guid SignalId { get; private set; }
    public string? BrokerTicket { get; private set; }
    public Guid SymbolId { get; private set; }
    public TradeType TradeType { get; private set; }
    public Price EntryPrice { get; private set; } = default!;
    public Price? ExitPrice { get; private set; }
    public Price StopLoss { get; private set; } = default!;
    public Price TakeProfit { get; private set; } = default!;
    public decimal PositionSize { get; private set; }
    public Money ProfitLoss { get; private set; } = Money.Zero();
    public TradeStatus Status { get; private set; }
    public DateTime OpenedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }

    private Trade() { }

    public static Trade Create(Guid signalId, Guid symbolId, TradeType tradeType,
        decimal entryPrice, decimal stopLoss, decimal takeProfit, decimal positionSize)
    {
        if (positionSize <= 0)
            throw new ArgumentException("Position size must be positive.");

        var trade = new Trade
        {
            SignalId = signalId,
            SymbolId = symbolId,
            TradeType = tradeType,
            EntryPrice = new Price(entryPrice),
            StopLoss = new Price(stopLoss),
            TakeProfit = new Price(takeProfit),
            PositionSize = positionSize,
            Status = TradeStatus.Pending,
            OpenedAt = DateTime.UtcNow
        };

        return trade;
    }

    public void Open(string brokerTicket)
    {
        if (Status != TradeStatus.Pending)
            throw new InvalidOperationException($"Cannot open trade in {Status} status.");

        BrokerTicket = brokerTicket;
        Status = TradeStatus.Open;
        SetUpdatedAt();
        AddDomainEvent(new TradeOpenedEvent(Id, SymbolId, EntryPrice.Value, PositionSize));
    }

    public void Close(decimal exitPrice, decimal profitLoss)
    {
        if (Status != TradeStatus.Open)
            throw new InvalidOperationException($"Cannot close trade in {Status} status.");

        ExitPrice = new Price(exitPrice);
        ProfitLoss = new Money(profitLoss);
        Status = TradeStatus.Closed;
        ClosedAt = DateTime.UtcNow;
        SetUpdatedAt();
        AddDomainEvent(new TradeClosedEvent(Id, profitLoss, ClosedAt.Value));
    }

    public void ModifyStopLoss(decimal newStopLoss)
    {
        if (Status != TradeStatus.Open)
            throw new InvalidOperationException("Can only modify open trades.");

        StopLoss = new Price(newStopLoss);
        SetUpdatedAt();
    }

    public void ModifyTakeProfit(decimal newTakeProfit)
    {
        if (Status != TradeStatus.Open)
            throw new InvalidOperationException("Can only modify open trades.");

        TakeProfit = new Price(newTakeProfit);
        SetUpdatedAt();
    }

    public void MarkAsFailed(string reason)
    {
        Status = TradeStatus.Failed;
        SetUpdatedAt();
        AddDomainEvent(new TradeExecutionFailedEvent(Id, reason));
    }
}