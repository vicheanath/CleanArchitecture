using Clean.Architecture.Domain.Inventory;
using Clean.Architecture.Domain.Orders;
using Shared.Messaging;

namespace Clean.Architecture.Application.Orders.EventHandlers;

/// <summary>
/// Domain event handler that fulfills inventory reservations when an order is shipped.
/// </summary>
internal sealed class OrderShippedDomainEventHandler : IDomainEventHandler<OrderShippedDomainEvent>
{
    private readonly IInventoryItemRepository _inventoryRepository;
    private readonly IOrderRepository _orderRepository;

    public OrderShippedDomainEventHandler(
        IInventoryItemRepository inventoryRepository,
        IOrderRepository orderRepository)
    {
        _inventoryRepository = inventoryRepository;
        _orderRepository = orderRepository;
    }

    public async Task Handle(OrderShippedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        // Get the order to access the items
        var order = await _orderRepository.GetByIdAsync(domainEvent.OrderId, cancellationToken);
        if (order == null)
        {
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
                continue;
            }

            // Fulfill the reservation - this will decrease actual stock and remove the reservation
            inventoryItem.ConfirmReservation($"Order-{domainEvent.OrderId.Value}");

            await _inventoryRepository.UpdateAsync(inventoryItem, cancellationToken);
        }
    }
}
