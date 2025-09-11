using Clean.Architecture.Application.Common.Interfaces;
using Clean.Architecture.Domain.Inventory;
using Clean.Architecture.Domain.Orders;
using Microsoft.Extensions.Logging;
using Shared.Messaging;

namespace Clean.Architecture.Application.EventHandlers.Orders;

/// <summary>
/// Handles OrderConfirmedDomainEvent to reserve inventory for the order
/// </summary>
public class OrderConfirmedDomainEventHandler : IDomainEventHandler<OrderConfirmedDomainEvent>
{
    private readonly IInventoryItemRepository _inventoryItemRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OrderConfirmedDomainEventHandler> _logger;

    public OrderConfirmedDomainEventHandler(
        IInventoryItemRepository inventoryItemRepository,
        IUnitOfWork unitOfWork,
        ILogger<OrderConfirmedDomainEventHandler> logger)
    {
        _inventoryItemRepository = inventoryItemRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(OrderConfirmedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling OrderConfirmedDomainEvent for order {OrderId}", domainEvent.OrderId.Value);

        try
        {
            foreach (var item in domainEvent.Items)
            {
                var inventoryItem = await _inventoryItemRepository.GetByProductSkuAsync(item.ProductSku, cancellationToken);
                if (inventoryItem == null)
                {
                    _logger.LogWarning("Inventory item not found for product SKU {ProductSku} in order {OrderId}",
                        item.ProductSku, domainEvent.OrderId.Value);
                    continue;
                }

                // Reserve inventory for the order
                var reservationId = $"ORDER-{domainEvent.OrderId.Value}-{item.ProductSku}";
                var expiresAt = DateTime.UtcNow.AddHours(24); // Reservation expires in 24 hours

                inventoryItem.ReserveStock(item.Quantity, reservationId, expiresAt);

                _logger.LogInformation("Reserved {Quantity} units of {ProductSku} for order {OrderId}",
                    item.Quantity, item.ProductSku, domainEvent.OrderId.Value);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Successfully reserved inventory for order {OrderId}", domainEvent.OrderId.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reserve inventory for order {OrderId}", domainEvent.OrderId.Value);
            throw;
        }
    }
}
