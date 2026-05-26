namespace ARABAZON.TradeAI.Domain.Entities;

public class SymbolRiskConfiguration
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid SymbolId { get; private set; }
    public decimal MaxRiskPerTrade { get; private set; }
    public decimal MaxDailyLoss { get; private set; }
    public decimal MaxPositionSize { get; private set; }
    public decimal MaxSpreadAllowed { get; private set; }
    public decimal MaxSlippageAllowed { get; private set; }
    public int MaxConcurrentTrades { get; private set; }
    public decimal AtrMultiplierSL { get; private set; }
    public decimal AtrMultiplierTP { get; private set; }
    public bool AutoTradingEnabled { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private SymbolRiskConfiguration() { }

    public static SymbolRiskConfiguration CreateForGold(Guid symbolId) => new()
    {
        SymbolId = symbolId,
        MaxRiskPerTrade = 1.0m,
        MaxDailyLoss = 3.0m,
        MaxPositionSize = 0.5m,
        MaxSpreadAllowed = 25m,
        MaxSlippageAllowed = 1.0m,
        MaxConcurrentTrades = 1,
        AtrMultiplierSL = 1.5m,
        AtrMultiplierTP = 2.5m,
        AutoTradingEnabled = false,
        UpdatedAt = DateTime.UtcNow,
    };

    public static SymbolRiskConfiguration CreateForOil(Guid symbolId) => new()
    {
        SymbolId = symbolId,
        MaxRiskPerTrade = 0.7m,
        MaxDailyLoss = 2.0m,
        MaxPositionSize = 1.0m,
        MaxSpreadAllowed = 8m,
        MaxSlippageAllowed = 0.08m,
        MaxConcurrentTrades = 1,
        AtrMultiplierSL = 1.5m,
        AtrMultiplierTP = 2.5m,
        AutoTradingEnabled = false,
        UpdatedAt = DateTime.UtcNow,
    };

    public void Update(
        decimal maxRiskPerTrade, decimal maxDailyLoss,
        decimal maxSpreadAllowed, decimal maxSlippageAllowed,
        int maxConcurrentTrades, bool autoTradingEnabled)
    {
        MaxRiskPerTrade = maxRiskPerTrade;
        MaxDailyLoss = maxDailyLoss;
        MaxSpreadAllowed = maxSpreadAllowed;
        MaxSlippageAllowed = maxSlippageAllowed;
        MaxConcurrentTrades = maxConcurrentTrades;
        AutoTradingEnabled = autoTradingEnabled;
        UpdatedAt = DateTime.UtcNow;
    }
}