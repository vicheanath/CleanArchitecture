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

        // Calculate percentage increase for analytics
        var percentageIncrease = domainEvent.PreviousQuantity > 0
            ? (domainEvent.QuantityAdded / (decimal)domainEvent.PreviousQuantity) * 100
            : 100;

        _logger.LogInformation(
            "Stock increase analytics - ProductSku: {ProductSku}, PercentageIncrease: {PercentageIncrease:F2}%, " +
            "StockLevelChange: {PreviousQuantity} -> {NewQuantity}",
            domainEvent.ProductSku,
            percentageIncrease,
            domainEvent.PreviousQuantity,
            domainEvent.NewQuantity);

        // Log if stock was replenished from zero or low levels
        if (domainEvent.PreviousQuantity == 0)
        {
            _logger.LogInformation(
                "Product {ProductSku} stock replenished from zero - {QuantityAdded} units added",
                domainEvent.ProductSku,
                domainEvent.QuantityAdded);
        }
        else if (domainEvent.PreviousQuantity <= 5)
        {
            _logger.LogInformation(
                "Product {ProductSku} stock replenished from low level - {QuantityAdded} units added, now at {NewQuantity}",
                domainEvent.ProductSku,
                domainEvent.QuantityAdded,
                domainEvent.NewQuantity);
        }

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

        // Calculate percentage decrease for analytics
        var percentageDecrease = domainEvent.PreviousQuantity > 0
            ? (domainEvent.QuantityRemoved / (decimal)domainEvent.PreviousQuantity) * 100
            : 0;

        _logger.LogInformation(
            "Stock decrease analytics - ProductSku: {ProductSku}, PercentageDecrease: {PercentageDecrease:F2}%, " +
            "StockLevelChange: {PreviousQuantity} -> {NewQuantity}",
            domainEvent.ProductSku,
            percentageDecrease,
            domainEvent.PreviousQuantity,
            domainEvent.NewQuantity);

        // Log if stock is now at zero or very low
        if (domainEvent.NewQuantity == 0)
        {
            _logger.LogWarning(
                "Product {ProductSku} stock depleted to zero after removing {QuantityRemoved} units",
                domainEvent.ProductSku,
                domainEvent.QuantityRemoved);
        }
        else if (domainEvent.NewQuantity <= 5)
        {
            _logger.LogWarning(
                "Product {ProductSku} stock is now critically low at {NewQuantity} units after removing {QuantityRemoved} units",
                domainEvent.ProductSku,
                domainEvent.NewQuantity,
                domainEvent.QuantityRemoved);
        }

        await Task.CompletedTask;
    }
}
