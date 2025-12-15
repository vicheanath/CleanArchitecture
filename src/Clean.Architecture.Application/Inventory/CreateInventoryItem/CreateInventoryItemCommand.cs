using Shared.Messaging;

namespace Clean.Architecture.Application.Inventory.CreateInventoryItem;

/// <summary>
/// Represents the create inventory item command.
/// </summary>
/// <param name="ProductSku">The product SKU.</param>
/// <param name="InitialQuantity">The initial quantity.</param>
/// <param name="MinimumStockLevel">The minimum stock level.</param>
public sealed record CreateInventoryItemCommand(
    string ProductSku,
    int InitialQuantity,
    int MinimumStockLevel) : ICommand<Guid>;
