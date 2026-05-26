using ARABAZON.TradeAI.Domain.Enums;

namespace ARABAZON.TradeAI.Domain.Entities;

public class RiskEvent
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public RiskEventType EventType { get; private set; }
    public string Severity { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public Guid? SymbolId { get; private set; }
    public decimal? CurrentValue { get; private set; }
    public decimal? ThresholdValue { get; private set; }
    public DateTime TriggeredAt { get; private set; }

    private RiskEvent() { }

    public static RiskEvent Create(
        RiskEventType eventType,
        string severity,
        string description,
        Guid? symbolId = null,
        decimal? currentValue = null,
        decimal? thresholdValue = null) => new()
        {
            EventType = eventType,
            Severity = severity,
            Description = description,
            SymbolId = symbolId,
            CurrentValue = currentValue,
            ThresholdValue = thresholdValue,
            TriggeredAt = DateTime.UtcNow,
        };
}