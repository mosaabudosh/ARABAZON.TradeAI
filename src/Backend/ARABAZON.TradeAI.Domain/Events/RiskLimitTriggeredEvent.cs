using ARABAZON.TradeAI.Domain.Common;
using ARABAZON.TradeAI.Domain.Enums;

namespace ARABAZON.TradeAI.Domain.Events;

public sealed class RiskLimitTriggeredEvent : BaseDomainEvent
{
    public RiskEventType RiskType { get; }
    public decimal CurrentExposure { get; }
    public decimal AllowedExposure { get; }

    public RiskLimitTriggeredEvent(RiskEventType riskType, decimal currentExposure, decimal allowedExposure)
    {
        RiskType = riskType;
        CurrentExposure = currentExposure;
        AllowedExposure = allowedExposure;
    }
}