namespace ARABAZON.TradeAI.Domain.Enums;

public enum RiskEventType
{
    DailyLossLimitReached,
    SpreadExplosion,
    ExecutionTimeout,
    AIUnavailable,
    CircuitBreakerTriggered,
    PositionMismatch,
    ExcessiveSlippage,
    ConnectionInstability,
    ExcessiveDrawdown
}