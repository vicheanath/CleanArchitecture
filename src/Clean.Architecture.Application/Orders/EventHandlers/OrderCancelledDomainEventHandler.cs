using Clean.Architecture.Application.Common.Interfaces;
using Clean.Architecture.Domain.Inventory;
using Clean.Architecture.Domain.Orders;
using Microsoft.Extensions.Logging;
using Shared.Messaging;

namespace Clean.Architecture.Application.Orders.EventHandlers;

/// <summary>
/// Handles OrderCancelledDomainEvent to release reserved inventory
/// </summary>
public class OrderCancelledDomainEventHandler : IDomainEventHandler<OrderCancelledDomainEvent>
{
    private readonly IInventoryItemRepository _inventoryItemRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OrderCancelledDomainEventHandler> _logger;

    public OrderCancelledDomainEventHandler(
        IInventoryItemRepository inventoryItemRepository,
        IUnitOfWork unitOfWork,
        ILogger<OrderCancelledDomainEventHandler> logger)
    {
        _inventoryItemRepository = inventoryItemRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(OrderCancelledDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling OrderCancelledDomainEvent for order {OrderId}", domainEvent.OrderId.Value);

        try
        {
            foreach (var item in domainEvent.Items)
            {
                var inventoryItem = await _inventoryItemRepository.GetByProductSkuAsync(item.ProductSku, cancellationToken);
                if (inventoryItem == null)
                {
                    _logger.LogWarning("Inventory item not found for product SKU {ProductSku} in cancelled order {OrderId}",
                        item.ProductSku, domainEvent.OrderId.Value);
                    continue;
                }

                // Release the reservation for this order
                var reservationId = $"ORDER-{domainEvent.OrderId.Value}-{item.ProductSku}";

                try
                {
                    inventoryItem.ReleaseReservation(reservationId);
                    _logger.LogInformation("Released reservation for {Quantity} units of {ProductSku} from cancelled order {OrderId}",
                        item.Quantity, item.ProductSku, domainEvent.OrderId.Value);
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogWarning("Could not release reservation {ReservationId}: {Message}",
                        reservationId, ex.Message);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Successfully released inventory reservations for cancelled order {OrderId}", domainEvent.OrderId.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to release inventory reservations for cancelled order {OrderId}", domainEvent.OrderId.Value);
            throw;
        }
    }
}
