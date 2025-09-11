using Shared.Primitives;

namespace Clean.Architecture.Domain.Inventory;

/// <summary>
/// Represents the inventory item identifier.
/// </summary>
/// <param name="Value">The identifier value.</param>
public sealed record InventoryItemId(Guid Value) : IEntityId
{
    /// <summary>
    /// Creates a new inventory item identifier.
    /// </summary>
    /// <returns>The newly created inventory item identifier.</returns>
    public static InventoryItemId New() => new(Guid.NewGuid());

    /// <summary>
    /// Creates an inventory item identifier from the specified value.
    /// </summary>
    /// <param name="value">The identifier value.</param>
    /// <returns>The inventory item identifier.</returns>
    public static InventoryItemId Create(Guid value) => new(value);
}
