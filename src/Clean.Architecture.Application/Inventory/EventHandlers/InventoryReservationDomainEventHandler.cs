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

        // Calculate reservation percentage of available stock
        var reservationPercentage = domainEvent.AvailableQuantity > 0
            ? (domainEvent.QuantityReserved / (decimal)domainEvent.AvailableQuantity) * 100
            : 0;

        _logger.LogInformation(
            "Reservation analytics - ProductSku: {ProductSku}, ReservationId: {ReservationId}, " +
            "ReservationPercentage: {ReservationPercentage:F2}% of available stock, " +
            "RemainingAvailable: {RemainingAvailable} units",
            domainEvent.ProductSku,
            domainEvent.ReservationId,
            reservationPercentage,
            domainEvent.AvailableQuantity - domainEvent.QuantityReserved);

        // Log if reservation leaves very little available stock
        var remainingAvailable = domainEvent.AvailableQuantity - domainEvent.QuantityReserved;
        if (remainingAvailable <= 5 && remainingAvailable > 0)
        {
            _logger.LogWarning(
                "Low available stock after reservation - ProductSku: {ProductSku}, " +
                "ReservationId: {ReservationId}, RemainingAvailable: {RemainingAvailable} units",
                domainEvent.ProductSku,
                domainEvent.ReservationId,
                remainingAvailable);
        }
        else if (remainingAvailable == 0)
        {
            _logger.LogWarning(
                "No available stock remaining after reservation - ProductSku: {ProductSku}, " +
                "ReservationId: {ReservationId}",
                domainEvent.ProductSku,
                domainEvent.ReservationId);
        }

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

        // Log the increase in available stock
        _logger.LogInformation(
            "Stock availability increased - ProductSku: {ProductSku}, ReservationId: {ReservationId}, " +
            "QuantityReleased: {QuantityReleased}, NewAvailableQuantity: {AvailableQuantity}",
            domainEvent.ProductSku,
            domainEvent.ReservationId,
            domainEvent.QuantityReleased,
            domainEvent.AvailableQuantity);

        // Log if stock is now back above minimum levels (if we had context about minimum)
        // This is informational for monitoring
        _logger.LogInformation(
            "Reservation release completed - ProductSku: {ProductSku}, ReservationId: {ReservationId}, " +
            "InventoryItemId: {InventoryItemId}, AvailableQuantity: {AvailableQuantity}",
            domainEvent.ProductSku,
            domainEvent.ReservationId,
            domainEvent.InventoryItemId.Value,
            domainEvent.AvailableQuantity);

        await Task.CompletedTask;
    }
}
