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
            "Inventory item created: {InventoryItemId} for product SKU: {ProductSku} with initial quantity: {InitialQuantity} and minimum stock level: {MinimumStockLevel}",
            domainEvent.InventoryItemId.Value,
            domainEvent.ProductSku,
            domainEvent.InitialQuantity,
            domainEvent.MinimumStockLevel);

        // Log structured information for monitoring and analytics
        _logger.LogInformation(
            "New inventory item registered - ProductSku: {ProductSku}, InventoryItemId: {InventoryItemId}, InitialQuantity: {InitialQuantity}, MinimumStockLevel: {MinimumStockLevel}",
            domainEvent.ProductSku,
            domainEvent.InventoryItemId.Value,
            domainEvent.InitialQuantity,
            domainEvent.MinimumStockLevel);

        // Check if initial quantity is below minimum and log warning
        if (domainEvent.InitialQuantity < domainEvent.MinimumStockLevel)
        {
            _logger.LogWarning(
                "Inventory item created with quantity below minimum stock level - ProductSku: {ProductSku}, Quantity: {Quantity}, MinimumLevel: {MinimumLevel}",
                domainEvent.ProductSku,
                domainEvent.InitialQuantity,
                domainEvent.MinimumStockLevel);
        }

        // Check if initial quantity is zero and log info
        if (domainEvent.InitialQuantity == 0)
        {
            _logger.LogInformation(
                "Inventory item created with zero initial quantity - ProductSku: {ProductSku}. Stock needs to be added.",
                domainEvent.ProductSku);
        }

        await Task.CompletedTask;
    }
}
