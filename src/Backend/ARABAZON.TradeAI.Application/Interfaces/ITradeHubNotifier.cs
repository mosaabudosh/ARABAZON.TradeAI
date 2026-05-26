namespace ARABAZON.TradeAI.Application.Interfaces;

public class TradeUpdate
{
    public Guid TradeId { get; init; }
    public string Symbol { get; init; } = default!;
    public decimal CurrentPrice { get; init; }
    public decimal UnrealizedPnL { get; init; }
}

public interface ITradeHubNotifier
{
    Task BroadcastTradeUpdateAsync(TradeUpdate update);
    Task BroadcastTradeClosedAsync(Guid tradeId, decimal finalPnL);
    Task BroadcastTradeOpenedAsync(Guid tradeId, string symbol, decimal entryPrice, decimal positionSize);
}