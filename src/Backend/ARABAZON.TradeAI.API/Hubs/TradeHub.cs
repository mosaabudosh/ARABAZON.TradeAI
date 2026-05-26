using Microsoft.AspNetCore.SignalR;

namespace ARABAZON.TradeAI.API.Hubs;

public class TradeHub : Hub
{
    public async Task SubscribeToTrades()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "trades");
    }
}