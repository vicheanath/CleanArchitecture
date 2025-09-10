using Clean.Architecture.Domain.Inventory.Events;
using Microsoft.Extensions.Logging;
using Shared.Messaging;

namespace Clean.Architecture.Application.Inventory.EventHandlers;

/// <summary>
/// Handles the low stock warning domain event.
/// </summary>
internal sealed class LowStockWarningDomainEventHandler : IDomainEventHandler<LowStockWarningDomainEvent>
{
    private readonly ILogger<LowStockWarningDomainEventHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LowStockWarningDomainEventHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public LowStockWarningDomainEventHandler(ILogger<LowStockWarningDomainEventHandler> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task Handle(LowStockWarningDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            "Low stock warning for inventory item {InventoryItemId}, product SKU: {ProductSku}. " +
            "Current quantity: {CurrentQuantity}, Minimum stock level: {MinimumStockLevel}",
            domainEvent.InventoryItemId.Value,
            domainEvent.ProductSku,
            domainEvent.CurrentQuantity,
            domainEvent.MinimumStockLevel);

        // Additional logic could be added here, such as:
        // - Sending email notifications to procurement team
        // - Creating purchase orders automatically
        // - Notifying suppliers
        // - Updating dashboard alerts
        // - Triggering reorder processes

        await Task.CompletedTask;
    }
}
