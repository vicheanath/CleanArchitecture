using Clean.Architecture.Domain.Inventory.Events;
using Microsoft.Extensions.Logging;
using Shared.Messaging;

namespace Clean.Architecture.Application.Inventory.EventHandlers;

/// <summary>
/// Handles OutOfStockDomainEvent to log critical warnings
/// </summary>
public class OutOfStockDomainEventHandler : IDomainEventHandler<OutOfStockDomainEvent>
{
    private readonly ILogger<OutOfStockDomainEventHandler> _logger;

    public OutOfStockDomainEventHandler(ILogger<OutOfStockDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(OutOfStockDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogCritical("CRITICAL: Product {ProductSku} is out of stock!", domainEvent.ProductSku);

        // Here you could implement additional logic such as:
        // - Send urgent notifications
        // - Disable product from online catalog
        // - Alert customer service team
        // - Trigger emergency reorder

        return Task.CompletedTask;
    }
}
