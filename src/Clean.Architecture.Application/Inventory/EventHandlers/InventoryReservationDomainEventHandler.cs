using Clean.Architecture.Domain.Inventory.Events;
using Microsoft.Extensions.Logging;
using Shared.Messaging;

namespace Clean.Architecture.Application.Inventory.EventHandlers;

/// <summary>
/// Handles inventory reservation domain events.
/// </summary>
internal sealed class InventoryReservationDomainEventHandler :
    IDomainEventHandler<InventoryReservedDomainEvent>,
    IDomainEventHandler<InventoryReservationReleasedDomainEvent>
{
    private readonly ILogger<InventoryReservationDomainEventHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="InventoryReservationDomainEventHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public InventoryReservationDomainEventHandler(ILogger<InventoryReservationDomainEventHandler> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task Handle(InventoryReservedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Inventory reserved for item {InventoryItemId}, product SKU: {ProductSku}. " +
            "Quantity reserved: {QuantityReserved}, Available quantity: {AvailableQuantity}, Reservation ID: {ReservationId}",
            domainEvent.InventoryItemId.Value,
            domainEvent.ProductSku,
            domainEvent.QuantityReserved,
            domainEvent.AvailableQuantity,
            domainEvent.ReservationId);

        // Additional logic could be added here, such as:
        // - Updating order management systems
        // - Notifying fulfillment centers
        // - Creating reservation audit logs
        // - Updating available inventory displays
        // - Triggering allocation workflows

        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task Handle(InventoryReservationReleasedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Inventory reservation released for item {InventoryItemId}, product SKU: {ProductSku}. " +
            "Quantity released: {QuantityReleased}, Available quantity: {AvailableQuantity}, Reservation ID: {ReservationId}",
            domainEvent.InventoryItemId.Value,
            domainEvent.ProductSku,
            domainEvent.QuantityReleased,
            domainEvent.AvailableQuantity,
            domainEvent.ReservationId);

        // Additional logic could be added here, such as:
        // - Updating order management systems
        // - Making inventory available for new orders
        // - Creating reservation release audit logs
        // - Updating available inventory displays
        // - Triggering inventory availability notifications

        await Task.CompletedTask;
    }
}
