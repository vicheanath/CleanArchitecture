using Clean.Architecture.Domain.Inventory.Events;
using Microsoft.Extensions.Logging;
using Shared.Messaging;

namespace Clean.Architecture.Application.EventHandlers.Inventory;

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
        _logger.LogWarning("Low stock warning for product {ProductSku}: Current quantity {CurrentQuantity}, Minimum level {MinimumLevel}",
            domainEvent.ProductSku, domainEvent.CurrentQuantity, domainEvent.MinimumStockLevel);

        // Here you could implement additional logic such as:
        // - Send notifications to procurement team
        // - Automatically create purchase orders
        // - Send email alerts
        // - Update external systems

        return Task.CompletedTask;
    }
}

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
