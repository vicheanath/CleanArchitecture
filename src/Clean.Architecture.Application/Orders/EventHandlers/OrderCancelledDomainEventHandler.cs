using Clean.Architecture.Domain.Inventory;
using Clean.Architecture.Domain.Orders;
using Shared.Messaging;

namespace Clean.Architecture.Application.Orders.EventHandlers;

/// <summary>
/// Domain event handler that releases inventory reservations when an order is cancelled.
/// </summary>
internal sealed class OrderCancelledDomainEventHandler : IDomainEventHandler<OrderCancelledDomainEvent>
{
    private readonly IInventoryItemRepository _inventoryRepository;

    public OrderCancelledDomainEventHandler(IInventoryItemRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    public async Task Handle(OrderCancelledDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        // Only release reservations if the order was previously confirmed
        if (domainEvent.PreviousStatus != OrderStatus.Confirmed)
        {
            return;
        }

        // Release inventory reservations for each order item
        foreach (var item in domainEvent.Items)
        {
            var inventoryItem = await _inventoryRepository.GetByProductSkuAsync(
                item.ProductSku,
                cancellationToken);

            if (inventoryItem == null)
            {
                continue;
            }

            // Release the reservation using the order ID as reference
            inventoryItem.ReleaseReservation($"Order-{domainEvent.OrderId.Value}");

            await _inventoryRepository.UpdateAsync(inventoryItem, cancellationToken);
        }
    }
}
