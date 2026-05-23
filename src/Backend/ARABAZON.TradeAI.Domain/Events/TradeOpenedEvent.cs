using ARABAZON.TradeAI.Domain.Common;

namespace ARABAZON.TradeAI.Domain.Events;

public sealed class TradeOpenedEvent : BaseDomainEvent
{
    public Guid TradeId { get; }
    public Guid SymbolId { get; }
    public decimal EntryPrice { get; }
    public decimal PositionSize { get; }

    public TradeOpenedEvent(Guid tradeId, Guid symbolId, decimal entryPrice, decimal positionSize)
    {
        TradeId = tradeId;
        SymbolId = symbolId;
        EntryPrice = entryPrice;
        PositionSize = positionSize;
    }
}