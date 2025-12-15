using Clean.Architecture.Domain.Inventory;
using Shared.Messaging;
using Shared.Results;

namespace Clean.Architecture.Application.Inventory.GetLowStockItems;

/// <summary>
/// Represents the get low stock items query handler.
/// </summary>
internal sealed class GetLowStockItemsQueryHandler : IQueryHandler<GetLowStockItemsQuery, IEnumerable<LowStockItemResponse>>
{
    private readonly IInventoryItemRepository _inventoryItemRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLowStockItemsQueryHandler"/> class.
    /// </summary>
    /// <param name="inventoryItemRepository">The inventory item repository.</param>
    public GetLowStockItemsQueryHandler(IInventoryItemRepository inventoryItemRepository)
    {
        _inventoryItemRepository = inventoryItemRepository;
    }

    /// <inheritdoc />
    public async Task<Result<IEnumerable<LowStockItemResponse>>> Handle(GetLowStockItemsQuery request, CancellationToken cancellationToken)
    {
        var lowStockItems = await _inventoryItemRepository.GetItemsBelowMinimumStockAsync(cancellationToken);

        var response = lowStockItems.Select(item => new LowStockItemResponse(
            item.Id.Value,
            item.ProductSku,
            item.Quantity,
            item.MinimumStockLevel,
            Math.Max(0, item.MinimumStockLevel - item.Quantity)));

        return Result.Success(response);
    }
}
