using ARABAZON.TradeAI.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace ARABAZON.TradeAI.API.Hubs;

public class TradeHubNotifier : ITradeHubNotifier
{
    private readonly IHubContext<TradeHub> _hub;
    public TradeHubNotifier(IHubContext<TradeHub> hub) => _hub = hub;

    public async Task BroadcastTradeUpdateAsync(TradeUpdate update) =>
        await _hub.Clients.Group("trades").SendAsync("ReceiveTradeUpdate", update);

    public async Task BroadcastTradeClosedAsync(Guid tradeId, decimal finalPnL) =>
        await _hub.Clients.Group("trades").SendAsync("ReceiveTradeClosed", new { tradeId, finalPnL });

    public async Task BroadcastTradeOpenedAsync(Guid tradeId, string symbol, decimal entryPrice, decimal positionSize) =>
        await _hub.Clients.Group("trades").SendAsync("ReceiveTradeOpened", new { tradeId, symbol, entryPrice, positionSize });
}