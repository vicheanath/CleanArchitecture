using Shared.Messaging;
using Shared.Results;

namespace Clean.Architecture.Application.Inventory.Queries.GetLowStockItems;

/// <summary>
/// Represents the low stock item response.
/// </summary>
/// <param name="Id">The inventory item identifier.</param>
/// <param name="ProductSku">The product SKU.</param>
/// <param name="Quantity">The current quantity.</param>
/// <param name="MinimumStockLevel">The minimum stock level.</param>
/// <param name="StockDeficit">The amount below minimum stock level.</param>
public sealed record LowStockItemResponse(
    Guid Id,
    string ProductSku,
    int Quantity,
    int MinimumStockLevel,
    int StockDeficit);

/// <summary>
/// Represents the get low stock items query.
/// </summary>
public sealed record GetLowStockItemsQuery() : IQuery<IEnumerable<LowStockItemResponse>>;
