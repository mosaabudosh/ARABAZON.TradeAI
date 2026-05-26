namespace ARABAZON.TradeAI.Application.Interfaces;

public interface IMarketHubNotifier
{
    Task BroadcastTickAsync(string symbol, decimal bid, decimal ask, decimal spread);
}