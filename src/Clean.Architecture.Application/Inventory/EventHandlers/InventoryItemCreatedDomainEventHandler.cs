using Clean.Architecture.Domain.Inventory.Events;
using Microsoft.Extensions.Logging;
using Shared.Messaging;

namespace Clean.Architecture.Application.Inventory.EventHandlers;

/// <summary>
/// Handles the inventory item created domain event.
/// </summary>
internal sealed class InventoryItemCreatedDomainEventHandler : IDomainEventHandler<InventoryItemCreatedDomainEvent>
{
    private readonly ILogger<InventoryItemCreatedDomainEventHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="InventoryItemCreatedDomainEventHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public InventoryItemCreatedDomainEventHandler(ILogger<InventoryItemCreatedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task Handle(InventoryItemCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Inventory item created: {InventoryItemId} for product SKU: {ProductSku} with initial quantity: {InitialQuantity}",
            domainEvent.InventoryItemId.Value,
            domainEvent.ProductSku,
            domainEvent.InitialQuantity);

        // Additional logic could be added here, such as:
        // - Sending notifications to warehouse staff
        // - Updating external inventory management systems
        // - Creating audit log entries
        // - Triggering other business processes

        await Task.CompletedTask;
    }
}
