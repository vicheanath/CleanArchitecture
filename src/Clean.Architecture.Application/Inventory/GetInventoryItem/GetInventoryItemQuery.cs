using Shared.Messaging;

namespace Clean.Architecture.Application.Inventory.GetInventoryItem;

/// <summary>
/// Represents the inventory item response.
/// </summary>
/// <param name="Id">The inventory item identifier.</param>
/// <param name="ProductSku">The product SKU.</param>
/// <param name="Quantity">The current quantity.</param>
/// <param name="ReservedQuantity">The reserved quantity.</param>
/// <param name="AvailableQuantity">The available quantity.</param>
/// <param name="MinimumStockLevel">The minimum stock level.</param>
/// <param name="IsOutOfStock">Indicates if the item is out of stock.</param>
/// <param name="IsBelowMinimumStock">Indicates if the item is below minimum stock level.</param>
/// <param name="CreatedOnUtc">The creation date and time.</param>
/// <param name="ModifiedOnUtc">The last modification date and time.</param>
public sealed record InventoryItemResponse(
    Guid Id,
    string ProductSku,
    int Quantity,
    int ReservedQuantity,
    int AvailableQuantity,
    int MinimumStockLevel,
    bool IsOutOfStock,
    bool IsBelowMinimumStock,
    DateTime CreatedOnUtc,
    DateTime? ModifiedOnUtc);

/// <summary>
/// Represents the get inventory item query.
/// </summary>
/// <param name="InventoryItemId">The inventory item identifier.</param>
public sealed record GetInventoryItemQuery(Guid InventoryItemId) : IQuery<InventoryItemResponse>;
