using ARABAZON.TradeAI.Domain.Entities;

namespace ARABAZON.TradeAI.Application.Interfaces;

public interface IStrategyEngine
{
    string StrategyName { get; }
    Task<StrategyResult> AnalyzeAsync(StrategyContext context, CancellationToken ct = default);
}

public class StrategyContext
{
    public string SymbolCode { get; init; } = default!;
    public Guid SymbolId { get; init; }
    public List<Candle> Candles { get; init; } = new();
}

public class StrategyResult
{
    public bool HasSignal { get; init; }
    public string SignalType { get; init; } = default!;   // "Buy" | "Sell"
    public decimal EntryPrice { get; init; }
    public decimal StopLoss { get; init; }
    public decimal TakeProfit { get; init; }
    public decimal ConfidenceScore { get; init; }         // 0–100
    public string Reason { get; init; } = default!;
    public Dictionary<string, decimal> IndicatorValues { get; init; } = new();

    public static StrategyResult NoSignal() => new() { HasSignal = false };
}