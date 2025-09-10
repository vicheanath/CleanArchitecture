using Clean.Architecture.Domain.Inventory.Events;
using Microsoft.Extensions.Logging;
using Shared.Messaging;

namespace Clean.Architecture.Application.Inventory.EventHandlers;

/// <summary>
/// Handles the inventory stock changed domain events.
/// </summary>
internal sealed class InventoryStockChangedDomainEventHandler :
    IDomainEventHandler<InventoryStockIncreasedDomainEvent>,
    IDomainEventHandler<InventoryStockDecreasedDomainEvent>
{
    private readonly ILogger<InventoryStockChangedDomainEventHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="InventoryStockChangedDomainEventHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public InventoryStockChangedDomainEventHandler(ILogger<InventoryStockChangedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task Handle(InventoryStockIncreasedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Inventory stock increased for item {InventoryItemId}, product SKU: {ProductSku}. " +
            "Quantity added: {QuantityAdded}, Previous: {PreviousQuantity}, New: {NewQuantity}. Reason: {Reason}",
            domainEvent.InventoryItemId.Value,
            domainEvent.ProductSku,
            domainEvent.QuantityAdded,
            domainEvent.PreviousQuantity,
            domainEvent.NewQuantity,
            domainEvent.Reason ?? "Not specified");

        // Additional logic could be added here, such as:
        // - Updating inventory reports
        // - Notifying sales team of increased availability
        // - Updating product availability on website
        // - Creating audit trail entries
        // - Triggering business intelligence updates

        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task Handle(InventoryStockDecreasedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Inventory stock decreased for item {InventoryItemId}, product SKU: {ProductSku}. " +
            "Quantity removed: {QuantityRemoved}, Previous: {PreviousQuantity}, New: {NewQuantity}. Reason: {Reason}",
            domainEvent.InventoryItemId.Value,
            domainEvent.ProductSku,
            domainEvent.QuantityRemoved,
            domainEvent.PreviousQuantity,
            domainEvent.NewQuantity,
            domainEvent.Reason ?? "Not specified");

        // Additional logic could be added here, such as:
        // - Updating inventory reports
        // - Checking if reorder is needed
        // - Updating product availability on website
        // - Creating audit trail entries
        // - Triggering business intelligence updates

        await Task.CompletedTask;
    }
}
