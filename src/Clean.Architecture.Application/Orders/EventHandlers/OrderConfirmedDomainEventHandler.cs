using Clean.Architecture.Domain.Inventory;
using Clean.Architecture.Domain.Orders;
using Shared.Messaging;

namespace Clean.Architecture.Application.Orders.EventHandlers;

/// <summary>
/// Domain event handler that reserves inventory when an order is confirmed.
/// </summary>
internal sealed class OrderConfirmedDomainEventHandler : IDomainEventHandler<OrderConfirmedDomainEvent>
{
    private readonly IInventoryItemRepository _inventoryRepository;

    public OrderConfirmedDomainEventHandler(IInventoryItemRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    public async Task Handle(OrderConfirmedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        // Reserve inventory for each order item
        foreach (var item in domainEvent.Items)
        {
            var inventoryItem = await _inventoryRepository.GetByProductSkuAsync(
                item.ProductSku,
                cancellationToken);

            if (inventoryItem == null)
            {
                // This could be a data integrity issue
                // In a real application, this might trigger a compensation action
                continue;
            }

            // Reserve the inventory with the order ID as reservation reference
            inventoryItem.ReserveStock(
                item.Quantity,
                $"Order-{domainEvent.OrderId.Value}",
                expiresAt: DateTime.UtcNow.AddDays(7)); // 7 days to fulfill

            await _inventoryRepository.UpdateAsync(inventoryItem, cancellationToken);
        }
    }
}
