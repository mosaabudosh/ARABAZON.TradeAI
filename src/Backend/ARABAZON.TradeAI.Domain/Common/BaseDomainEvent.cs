namespace ARABAZON.TradeAI.Domain.Common;

public abstract class BaseDomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}