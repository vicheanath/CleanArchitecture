using Clean.Architecture.Application.Common.Interfaces;
using Clean.Architecture.Domain.Inventory;
using Clean.Architecture.Domain.Products;
using Microsoft.Extensions.Logging;
using Shared.Messaging;

namespace Clean.Architecture.Application.EventHandlers.Products;

/// <summary>
/// Handles ProductCreatedDomainEvent to automatically create inventory item
/// </summary>
public class ProductCreatedDomainEventHandler : IDomainEventHandler<ProductCreatedDomainEvent>
{
    private readonly IInventoryItemRepository _inventoryItemRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProductCreatedDomainEventHandler> _logger;

    public ProductCreatedDomainEventHandler(
        IInventoryItemRepository inventoryItemRepository,
        IUnitOfWork unitOfWork,
        ILogger<ProductCreatedDomainEventHandler> logger)
    {
        _inventoryItemRepository = inventoryItemRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(ProductCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling ProductCreatedDomainEvent for product {ProductSku}", domainEvent.Sku);

        // Check if inventory item already exists for this SKU
        var existingInventoryItem = await _inventoryItemRepository.GetByProductSkuAsync(domainEvent.Sku, cancellationToken);
        if (existingInventoryItem != null)
        {
            _logger.LogWarning("Inventory item already exists for product SKU {ProductSku}", domainEvent.Sku);
            return;
        }

        // Create inventory item with default values
        var inventoryItem = InventoryItem.Create(
            productSku: domainEvent.Sku,
            initialQuantity: 0, // Start with 0 quantity
            minimumStockLevel: 5 // Default minimum stock level
        );

        await _inventoryItemRepository.AddAsync(inventoryItem, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created inventory item for product {ProductSku} with ID {InventoryItemId}",
            domainEvent.Sku, inventoryItem.Id.Value);
    }
}
