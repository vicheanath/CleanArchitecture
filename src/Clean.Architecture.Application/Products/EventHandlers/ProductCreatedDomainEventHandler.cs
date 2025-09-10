using Clean.Architecture.Domain.Inventory;
using Clean.Architecture.Domain.Products;
using Shared.Messaging;

namespace Clean.Architecture.Application.Products.EventHandlers;

/// <summary>
/// Domain event handler that creates inventory item when a product is created.
/// </summary>
internal sealed class ProductCreatedDomainEventHandler : IDomainEventHandler<ProductCreatedDomainEvent>
{
    private readonly IInventoryItemRepository _inventoryRepository;

    public ProductCreatedDomainEventHandler(IInventoryItemRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    public async Task Handle(ProductCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        // Check if inventory item already exists for this SKU
        var existingInventoryItem = await _inventoryRepository.GetByProductSkuAsync(
            domainEvent.Sku,
            cancellationToken);

        if (existingInventoryItem != null)
        {
            // Inventory already exists, nothing to do
            return;
        }

        // Create new inventory item with zero initial stock
        var inventoryItem = InventoryItem.Create(
            domainEvent.Sku,
            0, // Start with zero stock
            10); // Default minimum stock level

        await _inventoryRepository.AddAsync(inventoryItem, cancellationToken);
    }
}
