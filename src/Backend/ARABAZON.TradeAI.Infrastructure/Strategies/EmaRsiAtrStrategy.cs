using ARABAZON.TradeAI.Application.Interfaces;
using ARABAZON.TradeAI.Domain.Indicators;
using Microsoft.Extensions.Logging;

namespace ARABAZON.TradeAI.Infrastructure.Strategies;

public class EmaRsiAtrStrategy : IStrategyEngine
{
    private readonly ILogger<EmaRsiAtrStrategy> _logger;

    // Strategy parameters
    private const int EmaFast = 9;
    private const int EmaSlow = 21;
    private const int RsiPeriod = 14;
    private const int AtrPeriod = 14;
    private const decimal RsiOverbought = 65m;
    private const decimal RsiOversold = 35m;
    private const decimal MinConfidence = 55m;
    private const decimal AtrSlMultiplier = 1.5m;
    private const decimal AtrTpMultiplier = 2.5m;
    private const int MinCandles = 50;

    public string StrategyName => "EMA_RSI_ATR";

    public EmaRsiAtrStrategy(ILogger<EmaRsiAtrStrategy> logger)
    {
        _logger = logger;
    }

    public Task<StrategyResult> AnalyzeAsync(StrategyContext context, CancellationToken ct = default)
    {
        if (context.Candles.Count < MinCandles)
        {
            _logger.LogDebug("Not enough candles for {Symbol}: {Count}/{Min}",
                context.SymbolCode, context.Candles.Count, MinCandles);
            return Task.FromResult(StrategyResult.NoSignal());
        }

        var closes = context.Candles.Select(c => c.ClosePrice.Value).ToArray();
        var highs = context.Candles.Select(c => c.HighPrice.Value).ToArray();
        var lows = context.Candles.Select(c => c.LowPrice.Value).ToArray();

        // Calculate indicators
        var emaFast = EmaCalculator.Latest(closes, EmaFast);
        var emaSlow = EmaCalculator.Latest(closes, EmaSlow);
        var rsi = RsiCalculator.Latest(closes, RsiPeriod);
        var atr = AtrCalculator.Latest(highs, lows, closes, AtrPeriod);

        if (emaFast == null || emaSlow == null || rsi == null || atr == null)
            return Task.FromResult(StrategyResult.NoSignal());

        var currentPrice = closes[^1];
        var prevCloses = closes[..^1];
        var prevEmaFast = EmaCalculator.Latest(prevCloses, EmaFast);
        var prevEmaSlow = EmaCalculator.Latest(prevCloses, EmaSlow);

        if (prevEmaFast == null || prevEmaSlow == null)
            return Task.FromResult(StrategyResult.NoSignal());

        var indicators = new Dictionary<string, decimal>
        {
            ["EMA_FAST"] = emaFast.Value,
            ["EMA_SLOW"] = emaSlow.Value,
            ["RSI"] = rsi.Value,
            ["ATR"] = atr.Value,
        };

        // ── BUY Signal ───────────────────────────────────────
        bool emaFastCrossedAbove = prevEmaFast < prevEmaSlow && emaFast > emaSlow;
        bool rsiConfirmsBuy = rsi > 45m && rsi < RsiOverbought;
        bool priceAboveEmas = currentPrice > emaFast && currentPrice > emaSlow;

        if (emaFastCrossedAbove && rsiConfirmsBuy && priceAboveEmas)
        {
            var sl = currentPrice - atr.Value * AtrSlMultiplier;
            var tp = currentPrice + atr.Value * AtrTpMultiplier;
            var confidence = CalculateConfidence(rsi.Value, true, emaFast.Value, emaSlow.Value, currentPrice);

            if (confidence >= MinConfidence)
            {
                _logger.LogInformation("BUY signal for {Symbol} | Price={Price} SL={SL} TP={TP} RSI={RSI} Conf={Conf}",
                    context.SymbolCode, currentPrice, sl, tp, rsi.Value, confidence);

                return Task.FromResult(new StrategyResult
                {
                    HasSignal = true,
                    SignalType = "Buy",
                    EntryPrice = currentPrice,
                    StopLoss = Math.Round(sl, 5),
                    TakeProfit = Math.Round(tp, 5),
                    ConfidenceScore = confidence,
                    Reason = $"EMA crossover bullish | RSI={rsi.Value:F1} | ATR={atr.Value:F5}",
                    IndicatorValues = indicators,
                });
            }
        }

        // ── SELL Signal ──────────────────────────────────────
        bool emaFastCrossedBelow = prevEmaFast > prevEmaSlow && emaFast < emaSlow;
        bool rsiConfirmsSell = rsi < 55m && rsi > RsiOversold;
        bool priceBelowEmas = currentPrice < emaFast && currentPrice < emaSlow;

        if (emaFastCrossedBelow && rsiConfirmsSell && priceBelowEmas)
        {
            var sl = currentPrice + atr.Value * AtrSlMultiplier;
            var tp = currentPrice - atr.Value * AtrTpMultiplier;
            var confidence = CalculateConfidence(rsi.Value, false, emaFast.Value, emaSlow.Value, currentPrice);

            if (confidence >= MinConfidence)
            {
                _logger.LogInformation("SELL signal for {Symbol} | Price={Price} SL={SL} TP={TP} RSI={RSI} Conf={Conf}",
                    context.SymbolCode, currentPrice, sl, tp, rsi.Value, confidence);

                return Task.FromResult(new StrategyResult
                {
                    HasSignal = true,
                    SignalType = "Sell",
                    EntryPrice = currentPrice,
                    StopLoss = Math.Round(sl, 5),
                    TakeProfit = Math.Round(tp, 5),
                    ConfidenceScore = confidence,
                    Reason = $"EMA crossover bearish | RSI={rsi.Value:F1} | ATR={atr.Value:F5}",
                    IndicatorValues = indicators,
                });
            }
        }

        return Task.FromResult(StrategyResult.NoSignal());
    }

    private static decimal CalculateConfidence(
        decimal rsi, bool isBuy, decimal emaFast, decimal emaSlow, decimal price)
    {
        decimal score = 50m;

        // RSI strength
        if (isBuy)
        {
            if (rsi > 50 && rsi < 60) score += 15;
            else if (rsi >= 60 && rsi < 65) score += 10;
        }
        else
        {
            if (rsi < 50 && rsi > 40) score += 15;
            else if (rsi <= 40 && rsi > 35) score += 10;
        }

        // EMA separation strength
        decimal emaSeparation = Math.Abs(emaFast - emaSlow) / emaSlow * 100;
        if (emaSeparation > 0.1m) score += 10;
        if (emaSeparation > 0.2m) score += 5;

        // Price distance from slow EMA
        decimal priceVsEma = Math.Abs(price - emaSlow) / emaSlow * 100;
        if (priceVsEma < 0.5m) score += 10; // close to EMA = stronger signal
        else if (priceVsEma > 2m) score -= 10; // too far = weaker

        return Math.Min(Math.Max(score, 0), 100);
    }
}