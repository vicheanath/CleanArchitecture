using Clean.Architecture.Domain.Inventory.Events;
using Microsoft.Extensions.Logging;
using Shared.Messaging;

namespace Clean.Architecture.Application.Inventory.EventHandlers;

/// <summary>
/// Handles LowStockWarningDomainEvent to log warnings and potentially trigger reorder processes
/// </summary>
public class LowStockWarningDomainEventHandler : IDomainEventHandler<LowStockWarningDomainEvent>
{
    private readonly ILogger<LowStockWarningDomainEventHandler> _logger;

    public LowStockWarningDomainEventHandler(ILogger<LowStockWarningDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(LowStockWarningDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            "Low stock warning for product {ProductSku}: Current quantity {CurrentQuantity}, Minimum level {MinimumLevel}",
            domainEvent.ProductSku,
            domainEvent.CurrentQuantity,
            domainEvent.MinimumStockLevel);

        // Calculate stock deficit
        var stockDeficit = domainEvent.MinimumStockLevel - domainEvent.CurrentQuantity;
        var deficitPercentage = (stockDeficit / (decimal)domainEvent.MinimumStockLevel) * 100;

        _logger.LogWarning(
            "Low stock details - ProductSku: {ProductSku}, StockDeficit: {StockDeficit} units ({DeficitPercentage:F2}% below minimum), " +
            "CurrentQuantity: {CurrentQuantity}, MinimumStockLevel: {MinimumStockLevel}",
            domainEvent.ProductSku,
            stockDeficit,
            deficitPercentage,
            domainEvent.CurrentQuantity,
            domainEvent.MinimumStockLevel);

        // Log structured warning for monitoring systems
        _logger.LogWarning(
            "LOW_STOCK_ALERT - ProductSku: {ProductSku}, InventoryItemId: {InventoryItemId}, " +
            "CurrentQuantity: {CurrentQuantity}, MinimumStockLevel: {MinimumStockLevel}, StockDeficit: {StockDeficit}",
            domainEvent.ProductSku,
            domainEvent.InventoryItemId.Value,
            domainEvent.CurrentQuantity,
            domainEvent.MinimumStockLevel,
            stockDeficit);

        return Task.CompletedTask;
    }
}
