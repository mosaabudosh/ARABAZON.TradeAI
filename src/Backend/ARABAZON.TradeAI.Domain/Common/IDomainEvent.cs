namespace ARABAZON.TradeAI.Domain.Common;

public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
    Guid CorrelationId { get; }
}