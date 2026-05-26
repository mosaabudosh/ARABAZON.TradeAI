using ARABAZON.TradeAI.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace ARABAZON.TradeAI.API.Hubs;

public class MarketHubNotifier : IMarketHubNotifier
{
    private readonly IHubContext<MarketHub> _hub;

    public MarketHubNotifier(IHubContext<MarketHub> hub)
    {
        _hub = hub;
    }

    public async Task BroadcastTickAsync(string symbol, decimal bid, decimal ask, decimal spread)
    {
        await _hub.Clients.Group($"market-{symbol}").SendAsync("ReceiveTick", new
        {
            symbol,
            bid,
            ask,
            spread,
            timestamp = DateTime.UtcNow
        });
    }
}