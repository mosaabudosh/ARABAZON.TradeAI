using ARABAZON.TradeAI.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace ARABAZON.TradeAI.API.Hubs;

public class SignalHubNotifier : ISignalHubNotifier
{
    private readonly IHubContext<SignalHub> _hub;

    public SignalHubNotifier(IHubContext<SignalHub> hub) => _hub = hub;

    public async Task BroadcastSignalAsync(SignalNotification notification)
    {
        await _hub.Clients.Group("signals").SendAsync("ReceiveSignal", notification);
    }
}