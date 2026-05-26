namespace ARABAZON.TradeAI.Application.Interfaces;

public interface IPositionSizingService
{
    decimal Calculate(
        decimal accountBalance,
        decimal riskPercentage,
        decimal entryPrice,
        decimal stopLoss,
        decimal contractSize,
        decimal tickSize);
}