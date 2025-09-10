using Shared.Messaging;

namespace Clean.Architecture.Application.Inventory.Commands.AdjustInventoryStock;

/// <summary>
/// Represents the adjust inventory stock command.
/// </summary>
/// <param name="InventoryItemId">The inventory item identifier.</param>
/// <param name="QuantityChange">The quantity change (positive for increase, negative for decrease).</param>
/// <param name="Reason">The reason for the adjustment.</param>
public sealed record AdjustInventoryStockCommand(
    Guid InventoryItemId,
    int QuantityChange,
    string? Reason) : ICommand;
