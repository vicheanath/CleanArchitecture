using Clean.Architecture.Application.Common.Interfaces;
using Clean.Architecture.Domain.Inventory;
using Clean.Architecture.Domain.Orders;
using Microsoft.Extensions.Logging;
using Shared.Messaging;

namespace Clean.Architecture.Application.Orders.EventHandlers;

/// <summary>
/// Domain event handler that fulfills inventory reservations when an order is shipped.
/// </summary>
internal sealed class OrderShippedDomainEventHandler : IDomainEventHandler<OrderShippedDomainEvent>
{
    private readonly IInventoryItemRepository _inventoryRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OrderShippedDomainEventHandler> _logger;

    public OrderShippedDomainEventHandler(
        IInventoryItemRepository inventoryRepository,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        ILogger<OrderShippedDomainEventHandler> logger)
    {
        _inventoryRepository = inventoryRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(OrderShippedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling OrderShippedDomainEvent for order {OrderId}", domainEvent.OrderId.Value);

        try
        {
            // Get the order to access the items
            var order = await _orderRepository.GetByIdAsync(domainEvent.OrderId, cancellationToken);
            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found when processing shipment", domainEvent.OrderId.Value);
                return;
            }

            // Fulfill inventory reservations for each order item
            foreach (var item in order.Items)
            {
                var inventoryItem = await _inventoryRepository.GetByProductSkuAsync(
                    item.ProductSku,
                    cancellationToken);

                if (inventoryItem == null)
                {
                    _logger.LogWarning("Inventory item not found for product SKU {ProductSku} when shipping order {OrderId}",
                        item.ProductSku, domainEvent.OrderId.Value);
                    continue;
                }

                // Use the same reservation ID format as OrderConfirmedDomainEventHandler
                var reservationId = $"ORDER-{domainEvent.OrderId.Value}-{item.ProductSku}";

                try
                {
                    // Fulfill the reservation - this will decrease actual stock and remove the reservation
                    inventoryItem.ConfirmReservation(reservationId, $"Order {domainEvent.OrderId.Value} shipped");
                    await _inventoryRepository.UpdateAsync(inventoryItem, cancellationToken);

                    _logger.LogInformation("Confirmed reservation for {Quantity} units of {ProductSku} for shipped order {OrderId}",
                        item.Quantity, item.ProductSku, domainEvent.OrderId.Value);
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogWarning("Could not confirm reservation {ReservationId} for order {OrderId}: {Message}",
                        reservationId, domainEvent.OrderId.Value, ex.Message);
                }
                catch (ArgumentException ex)
                {
                    _logger.LogWarning("Invalid reservation {ReservationId} for order {OrderId}: {Message}",
                        reservationId, domainEvent.OrderId.Value, ex.Message);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Successfully fulfilled inventory reservations for shipped order {OrderId}", domainEvent.OrderId.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fulfill inventory reservations for shipped order {OrderId}", domainEvent.OrderId.Value);
            throw;
        }
    }
}
