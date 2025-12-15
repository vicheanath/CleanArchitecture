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
        _logger.LogCritical(
            "CRITICAL: Product {ProductSku} is out of stock! InventoryItemId: {InventoryItemId}",
            domainEvent.ProductSku,
            domainEvent.InventoryItemId.Value);

        // Log structured critical alert for monitoring systems
        _logger.LogCritical(
            "OUT_OF_STOCK_ALERT - ProductSku: {ProductSku}, InventoryItemId: {InventoryItemId}, " +
            "Timestamp: {Timestamp}",
            domainEvent.ProductSku,
            domainEvent.InventoryItemId.Value,
            domainEvent.OccurredOnUtc);

        // Log additional context for operations team
        _logger.LogCritical(
            "URGENT: Immediate action required - Product {ProductSku} (InventoryItemId: {InventoryItemId}) " +
            "has reached zero stock. Restocking required immediately.",
            domainEvent.ProductSku,
            domainEvent.InventoryItemId.Value);

        return Task.CompletedTask;
    }
}
