using ARABAZON.TradeAI.Application.Interfaces;
using ARABAZON.TradeAI.Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ARABAZON.TradeAI.Infrastructure.Services;

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IMediator _mediator;
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(IMediator mediator, ILogger<DomainEventDispatcher> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in events)
        {
            _logger.LogInformation("Dispatching domain event {EventType} with Id {EventId}",
                domainEvent.GetType().Name, domainEvent.EventId);

            await _mediator.Publish(domainEvent, cancellationToken);
        }
    }
}