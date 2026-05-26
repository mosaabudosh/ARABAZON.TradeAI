using ARABAZON.TradeAI.Application.Interfaces;
using ARABAZON.TradeAI.Domain.Entities;
using ARABAZON.TradeAI.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ARABAZON.TradeAI.Workers;

public class SignalScannerWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SignalScannerWorker> _logger;

    private static readonly string[] Symbols = ["XAUUSD", "USOIL"];
    private static readonly Timeframe PrimaryTimeframe = Timeframe.M15;
    private readonly TimeSpan _scanInterval = TimeSpan.FromMinutes(1);

    public SignalScannerWorker(IServiceScopeFactory scopeFactory, ILogger<SignalScannerWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SignalScannerWorker started");

        // Wait for market data to be available first
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ScanAllSymbolsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SignalScannerWorker error");
            }

            await Task.Delay(_scanInterval, stoppingToken);
        }

        _logger.LogInformation("SignalScannerWorker stopped");
    }

    private async Task ScanAllSymbolsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var candleRepo = scope.ServiceProvider.GetRequiredService<ICandleRepository>();
        var strategyFactory = scope.ServiceProvider.GetRequiredService<IStrategyEngineFactory>();
        var signalNotifier = scope.ServiceProvider.GetRequiredService<ISignalHubNotifier>();

        foreach (var symbolCode in Symbols)
        {
            var symbol = await db.Symbols
                .FirstOrDefaultAsync(s => s.SymbolCode == symbolCode, ct);

            if (symbol == null) continue;

            // Load last 100 candles
            var from = DateTime.UtcNow.AddHours(-48);
            var candles = (await candleRepo.GetCandlesAsync(
                symbol.Id, PrimaryTimeframe, from, DateTime.UtcNow, ct))
                .OrderBy(c => c.OpenTime)
                .ToList();

            if (candles.Count < 30)
            {
                _logger.LogDebug("Not enough candles for {Symbol}", symbolCode);
                continue;
            }

            var context = new StrategyContext
            {
                SymbolCode = symbolCode,
                SymbolId = symbol.Id,
                Candles = candles,
            };

            // Run all active strategies
            foreach (var strategy in strategyFactory.GetActiveStrategies())
            {
                var result = await strategy.AnalyzeAsync(context, ct);

                if (!result.HasSignal) continue;

                // Check for duplicate signal in last 30 min
                var recentSignal = await db.TradeSignals.AnyAsync(s =>
                    s.SymbolId == symbol.Id &&
                    s.Status == SignalStatus.Pending &&
                    s.GeneratedAt > DateTime.UtcNow.AddMinutes(-30), ct);

                if (recentSignal)
                {
                    _logger.LogDebug("Duplicate signal suppressed for {Symbol}", symbolCode);
                    continue;
                }

                // Persist signal
                var signalType = result.SignalType == "Buy" ? SignalType.Buy : SignalType.Sell;
                var signal = TradeSignal.Create(
                    symbol.Id,
                    signalType,
                    strategy.StrategyName,
                    result.EntryPrice,
                    result.StopLoss,
                    result.TakeProfit,
                    result.ConfidenceScore,
                    1.0m  // default risk %
                );

                await db.TradeSignals.AddAsync(signal, ct);
                await db.SaveChangesAsync(ct);

                _logger.LogInformation(
                    "Signal saved: {Type} {Symbol} @ {Price} SL={SL} TP={TP} Conf={Conf}%",
                    result.SignalType, symbolCode, result.EntryPrice,
                    result.StopLoss, result.TakeProfit, result.ConfidenceScore);

                // Broadcast via SignalR
                await signalNotifier.BroadcastSignalAsync(new SignalNotification
                {
                    SignalId = signal.Id,
                    Symbol = symbolCode,
                    SignalType = result.SignalType,
                    EntryPrice = result.EntryPrice,
                    StopLoss = result.StopLoss,
                    TakeProfit = result.TakeProfit,
                    ConfidenceScore = result.ConfidenceScore,
                    Reason = result.Reason,
                    GeneratedAt = signal.GeneratedAt,
                });
            }
        }
    }
}