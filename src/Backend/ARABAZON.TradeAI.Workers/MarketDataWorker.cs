using ARABAZON.TradeAI.Application.Interfaces;
using ARABAZON.TradeAI.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ARABAZON.TradeAI.Workers;

public class MarketDataWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MarketDataWorker> _logger;

    private static readonly string[] Symbols = ["XAUUSD", "USOIL"];
    private static readonly Timeframe[] Timeframes = [Timeframe.M5, Timeframe.M15];
    private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(30);

    public MarketDataWorker(IServiceScopeFactory scopeFactory, ILogger<MarketDataWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MarketDataWorker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PollMarketDataAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MarketDataWorker error");
            }

            await Task.Delay(_pollInterval, stoppingToken);
        }

        _logger.LogInformation("MarketDataWorker stopped");
    }

    private async Task PollMarketDataAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var marketDataService = scope.ServiceProvider.GetRequiredService<IMarketDataService>();
        var candleRepository = scope.ServiceProvider.GetRequiredService<ICandleRepository>();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        // Check MT5 connection
        if (!await marketDataService.IsConnectedAsync(ct))
        {
            _logger.LogWarning("MT5 not connected — skipping poll");
            return;
        }

        foreach (var symbolCode in Symbols)
        {
            // Resolve SymbolId from DB
            var symbol = await dbContext.Symbols
                .FirstOrDefaultAsync(s => s.SymbolCode == symbolCode, ct);

            if (symbol == null)
            {
                _logger.LogWarning("Symbol {Symbol} not found in DB — skipping", symbolCode);
                continue;
            }

            // Fetch and store candles for each timeframe
            foreach (var timeframe in Timeframes)
            {
                var candles = (await marketDataService.GetCandlesAsync(
                    symbolCode, timeframe, 50, ct)).ToList();

                // Attach real SymbolId
                var typed = candles.Select(c => Domain.Entities.Candle.Create(
                    symbol.Id, c.Timeframe,
                    c.OpenPrice.Value, c.HighPrice.Value,
                    c.LowPrice.Value, c.ClosePrice.Value,
                    c.Volume, c.OpenTime, c.CloseTime
                ));

                await candleRepository.SaveCandlesAsync(typed, ct);
                _logger.LogDebug("Polled {Count} candles for {Symbol} {Timeframe}",
                    candles.Count, symbolCode, timeframe);
            }

            // Broadcast live snapshot via SignalR
            try
            {
                var snapshot = await marketDataService.GetLiveSnapshotAsync(symbolCode, ct);
                // SignalR broadcast — wired in next step
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get live snapshot for {Symbol}", symbolCode);
            }
        }
    }
}