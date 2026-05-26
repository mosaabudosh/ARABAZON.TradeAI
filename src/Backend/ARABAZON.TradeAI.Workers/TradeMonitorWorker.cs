using ARABAZON.TradeAI.Application.Interfaces;
using ARABAZON.TradeAI.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ARABAZON.TradeAI.Workers;

public class TradeMonitorWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TradeMonitorWorker> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(5);

    public TradeMonitorWorker(IServiceScopeFactory scopeFactory, ILogger<TradeMonitorWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TradeMonitorWorker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SyncPositionsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TradeMonitorWorker error");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task SyncPositionsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var mt5 = scope.ServiceProvider.GetRequiredService<IMT5ExecutionAdapter>();
        var dailyTracker = scope.ServiceProvider.GetRequiredService<IDailyRiskTracker>();
        var tradeHub = scope.ServiceProvider.GetRequiredService<ITradeHubNotifier>();

        // Load open trades from DB
        var openTrades = await db.Trades
            .Where(t => t.Status == TradeStatus.Open)
            .ToListAsync(ct);

        if (!openTrades.Any()) return;

        // Load live positions from MT5
        var mt5Positions = (await mt5.GetPositionsAsync(ct)).ToList();
        var mt5Tickets = mt5Positions.Select(p => p.Ticket).ToHashSet();

        foreach (var trade in openTrades)
        {
            if (string.IsNullOrEmpty(trade.BrokerTicket)) continue;

            var mt5Pos = mt5Positions.FirstOrDefault(p => p.Ticket == trade.BrokerTicket);

            if (mt5Pos == null)
            {
                // Position closed externally (SL/TP hit or manual close on MT5)
                _logger.LogWarning(
                    "Trade {TradeId} ticket {Ticket} not found in MT5 — closing in DB",
                    trade.Id, trade.BrokerTicket);

                decimal pnl = 0; // MT5 already closed it
                trade.Close(trade.EntryPrice.Value, pnl);
                dailyTracker.RecordTrade(pnl);

                await db.SaveChangesAsync(ct);
                await tradeHub.BroadcastTradeClosedAsync(trade.Id, pnl);
                continue;
            }

            // Update Position snapshot
            var position = await db.Positions
                .FirstOrDefaultAsync(p => p.TradeId == trade.Id, ct);

            if (position != null)
            {
                position.Update(mt5Pos.CurrentPrice, mt5Pos.Profit);
                await db.SaveChangesAsync(ct);
            }

            // Broadcast live update
            await tradeHub.BroadcastTradeUpdateAsync(new TradeUpdate
            {
                TradeId = trade.Id,
                Symbol = mt5Pos.Symbol,
                CurrentPrice = mt5Pos.CurrentPrice,
                UnrealizedPnL = mt5Pos.Profit,
            });
        }
    }
}