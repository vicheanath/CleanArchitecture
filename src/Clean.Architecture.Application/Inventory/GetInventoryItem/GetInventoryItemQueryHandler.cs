using Clean.Architecture.Domain.Inventory;
using Shared.Messaging;
using Shared.Results;

namespace Clean.Architecture.Application.Inventory.GetInventoryItem;

/// <summary>
/// Represents the get inventory item query handler.
/// </summary>
internal sealed class GetInventoryItemQueryHandler : IQueryHandler<GetInventoryItemQuery, InventoryItemResponse>
{
    private readonly IInventoryItemRepository _inventoryItemRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetInventoryItemQueryHandler"/> class.
    /// </summary>
    /// <param name="inventoryItemRepository">The inventory item repository.</param>
    public GetInventoryItemQueryHandler(IInventoryItemRepository inventoryItemRepository)
    {
        _inventoryItemRepository = inventoryItemRepository;
    }

    /// <inheritdoc />
    public async Task<Result<InventoryItemResponse>> Handle(GetInventoryItemQuery request, CancellationToken cancellationToken)
    {
        var inventoryItemId = InventoryItemId.Create(request.InventoryItemId);

        var inventoryItem = await _inventoryItemRepository.GetByIdAsync(inventoryItemId, cancellationToken);
        if (inventoryItem is null)
        {
            return Result.Failure<InventoryItemResponse>(InventoryErrors.NotFound);
        }

        var response = new InventoryItemResponse(
            inventoryItem.Id.Value,
            inventoryItem.ProductSku,
            inventoryItem.Quantity,
            inventoryItem.ReservedQuantity,
            inventoryItem.AvailableQuantity,
            inventoryItem.MinimumStockLevel,
            inventoryItem.IsOutOfStock,
            inventoryItem.IsBelowMinimumStock,
            inventoryItem.CreatedOnUtc,
            inventoryItem.ModifiedOnUtc);

        return Result.Success(response);
    }
}
