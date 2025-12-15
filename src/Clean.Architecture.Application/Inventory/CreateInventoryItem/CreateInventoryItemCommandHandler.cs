using Clean.Architecture.Domain.Inventory;
using Shared.Messaging;
using Shared.Results;

namespace Clean.Architecture.Application.Inventory.CreateInventoryItem;

/// <summary>
/// Represents the create inventory item command handler.
/// </summary>
internal sealed class CreateInventoryItemCommandHandler : ICommandHandler<CreateInventoryItemCommand, Guid>
{
    private readonly IInventoryItemRepository _inventoryItemRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateInventoryItemCommandHandler"/> class.
    /// </summary>
    /// <param name="inventoryItemRepository">The inventory item repository.</param>
    public CreateInventoryItemCommandHandler(IInventoryItemRepository inventoryItemRepository)
    {
        _inventoryItemRepository = inventoryItemRepository;
    }

    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(CreateInventoryItemCommand request, CancellationToken cancellationToken)
    {
        // Check if an inventory item with the same product SKU already exists
        var existingItem = await _inventoryItemRepository.GetByProductSkuAsync(request.ProductSku, cancellationToken);
        if (existingItem is not null)
        {
            return Result.Failure<Guid>(InventoryErrors.DuplicateProductSku);
        }

        // Validate input parameters
        if (string.IsNullOrWhiteSpace(request.ProductSku))
        {
            return Result.Failure<Guid>(InventoryErrors.ProductSkuRequired);
        }

        if (request.InitialQuantity < 0)
        {
            return Result.Failure<Guid>(InventoryErrors.InvalidQuantity);
        }

        if (request.MinimumStockLevel < 0)
        {
            return Result.Failure<Guid>(InventoryErrors.InvalidMinimumStockLevel);
        }

        // Create the inventory item
        var inventoryItem = InventoryItem.Create(
            request.ProductSku,
            request.InitialQuantity,
            request.MinimumStockLevel);

        // Add to repository
        _inventoryItemRepository.Add(inventoryItem);

        return Result.Success(inventoryItem.Id.Value);
    }
}
