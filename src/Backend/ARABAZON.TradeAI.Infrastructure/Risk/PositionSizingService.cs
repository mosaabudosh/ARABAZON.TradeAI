using ARABAZON.TradeAI.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace ARABAZON.TradeAI.Infrastructure.Risk;

public class PositionSizingService : IPositionSizingService
{
    private readonly ILogger<PositionSizingService> _logger;

    // Broker minimum lot sizes
    private const decimal MinLotSize = 0.01m;
    private const decimal MaxLotSize = 10.0m;
    private const decimal LotStep = 0.01m;

    public PositionSizingService(ILogger<PositionSizingService> logger)
    {
        _logger = logger;
    }

    public decimal Calculate(
        decimal accountBalance,
        decimal riskPercentage,
        decimal entryPrice,
        decimal stopLoss,
        decimal contractSize,
        decimal tickSize)
    {
        if (accountBalance <= 0 || riskPercentage <= 0)
            return MinLotSize;

        decimal riskAmount = accountBalance * (riskPercentage / 100m);
        decimal stopDistance = Math.Abs(entryPrice - stopLoss);

        if (stopDistance <= 0)
        {
            _logger.LogWarning("Stop distance is zero — returning min lot");
            return MinLotSize;
        }

        // Lot size = RiskAmount / (StopDistance × ContractSize)
        decimal lotSize = riskAmount / (stopDistance * contractSize);

        // Round down to lot step
        lotSize = Math.Floor(lotSize / LotStep) * LotStep;

        // Clamp
        lotSize = Math.Max(MinLotSize, Math.Min(MaxLotSize, lotSize));

        _logger.LogDebug(
            "Position sizing: Balance={Balance} Risk={Risk}% Amount={Amount} StopDist={Dist} Lots={Lots}",
            accountBalance, riskPercentage, riskAmount, stopDistance, lotSize);

        return lotSize;
    }
}