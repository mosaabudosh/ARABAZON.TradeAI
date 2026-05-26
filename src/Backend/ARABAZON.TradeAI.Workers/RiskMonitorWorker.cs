using ARABAZON.TradeAI.Application.Interfaces;
using ARABAZON.TradeAI.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ARABAZON.TradeAI.Workers;

public class RiskMonitorWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RiskMonitorWorker> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(10);

    public RiskMonitorWorker(IServiceScopeFactory scopeFactory, ILogger<RiskMonitorWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RiskMonitorWorker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await MonitorAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RiskMonitorWorker error");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task MonitorAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var dailyTracker = scope.ServiceProvider.GetRequiredService<IDailyRiskTracker>();
        var marketDataService = scope.ServiceProvider.GetRequiredService<IMarketDataService>();

        if (dailyTracker.IsEmergencyStop)
        {
            _logger.LogWarning("Emergency stop active — all auto-trading suspended");
            return;
        }

        // Monitor open trades for floating P&L
        var openTrades = await db.Trades
            .Where(t => t.Status == TradeStatus.Open)
            .ToListAsync(ct);

        if (!openTrades.Any()) return;

        foreach (var trade in openTrades)
        {
            try
            {
                var symbol = await db.Symbols.FindAsync([trade.SymbolId], ct);
                if (symbol == null) continue;

                var snapshot = await marketDataService.GetLiveSnapshotAsync(symbol.SymbolCode, ct);
                var currentPrice = trade.TradeType == TradeType.Buy
                    ? snapshot.BidPrice
                    : snapshot.AskPrice;

                // Check if SL hit (simple check — MT5 handles actual execution)
                bool slHit = trade.TradeType == TradeType.Buy
                    ? currentPrice <= trade.StopLoss.Value
                    : currentPrice >= trade.StopLoss.Value;

                if (slHit)
                {
                    _logger.LogWarning(
                        "SL may be hit for trade {TradeId} | Current={Price} SL={SL}",
                        trade.Id, currentPrice, trade.StopLoss.Value);
                }

                _logger.LogDebug(
                    "Trade {TradeId} | Symbol={Symbol} Current={Price}",
                    trade.Id, symbol.SymbolCode, currentPrice);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to monitor trade {TradeId}", trade.Id);
            }
        }
    }
}