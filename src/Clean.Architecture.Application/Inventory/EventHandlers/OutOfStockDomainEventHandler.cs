using Clean.Architecture.Domain.Inventory.Events;
using Microsoft.Extensions.Logging;
using Shared.Messaging;

namespace Clean.Architecture.Application.Inventory.EventHandlers;

/// <summary>
/// Handles the out of stock domain event.
/// </summary>
internal sealed class OutOfStockDomainEventHandler : IDomainEventHandler<OutOfStockDomainEvent>
{
    private readonly ILogger<OutOfStockDomainEventHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OutOfStockDomainEventHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public OutOfStockDomainEventHandler(ILogger<OutOfStockDomainEventHandler> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task Handle(OutOfStockDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogCritical(
            "Product is out of stock! Inventory item {InventoryItemId}, product SKU: {ProductSku}",
            domainEvent.InventoryItemId.Value,
            domainEvent.ProductSku);

        // Additional logic could be added here, such as:
        // - Sending urgent notifications to management
        // - Disabling product sales on the website
        // - Creating high-priority purchase orders
        // - Notifying customer service team
        // - Updating product availability status
        // - Triggering emergency restocking procedures

        await Task.CompletedTask;
    }
}
