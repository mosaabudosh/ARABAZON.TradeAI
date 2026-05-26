using Microsoft.AspNetCore.SignalR;

namespace ARABAZON.TradeAI.API.Hubs;

public class SignalHub : Hub
{
    public async Task SubscribeToSignals()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "signals");
    }
}