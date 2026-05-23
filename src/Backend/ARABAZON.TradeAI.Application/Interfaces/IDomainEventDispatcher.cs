using ARABAZON.TradeAI.Domain.Common;

namespace ARABAZON.TradeAI.Application.Interfaces;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default);
}