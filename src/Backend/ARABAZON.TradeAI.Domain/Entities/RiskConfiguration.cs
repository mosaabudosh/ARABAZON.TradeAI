using ARABAZON.TradeAI.Domain.Common;

namespace ARABAZON.TradeAI.Domain.Entities;

public class RiskConfiguration : BaseEntity
{
    public decimal MaxRiskPerTrade { get; private set; }
    public decimal DailyLossLimit { get; private set; }
    public int MaxConcurrentTrades { get; private set; }
    public decimal MaxExposure { get; private set; }
    public bool AutoTradingEnabled { get; private set; }

    private RiskConfiguration() { }

    public static RiskConfiguration CreateDefault() => new()
    {
        MaxRiskPerTrade = 1.0m,
        DailyLossLimit = 3.0m,
        MaxConcurrentTrades = 2,
        MaxExposure = 5.0m,
        AutoTradingEnabled = false
    };

    public void Update(decimal maxRiskPerTrade, decimal dailyLossLimit,
        int maxConcurrentTrades, decimal maxExposure)
    {
        if (maxRiskPerTrade is <= 0 or > 5)
            throw new ArgumentException("Max risk per trade must be between 0 and 5%.");
        if (dailyLossLimit is <= 0 or > 10)
            throw new ArgumentException("Daily loss limit must be between 0 and 10%.");

        MaxRiskPerTrade = maxRiskPerTrade;
        DailyLossLimit = dailyLossLimit;
        MaxConcurrentTrades = maxConcurrentTrades;
        MaxExposure = maxExposure;
        SetUpdatedAt();
    }

    public void EnableAutoTrading() { AutoTradingEnabled = true; SetUpdatedAt(); }
    public void DisableAutoTrading() { AutoTradingEnabled = false; SetUpdatedAt(); }
}