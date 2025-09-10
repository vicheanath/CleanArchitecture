using Shared.Primitives;

namespace Clean.Architecture.Domain.Inventory;

/// <summary>
/// Represents the inventory item identifier.
/// </summary>
public sealed class InventoryItemId : IEntityId
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InventoryItemId"/> class.
    /// </summary>
    /// <param name="value">The identifier value.</param>
    public InventoryItemId(Guid value) => Value = value;

    /// <summary>
    /// Gets the identifier value.
    /// </summary>
    public Guid Value { get; }

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

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is InventoryItemId other && Value.Equals(other.Value);
    }

    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode();

    /// <inheritdoc />
    public override string ToString() => Value.ToString();

    public static bool operator ==(InventoryItemId? left, InventoryItemId? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(InventoryItemId? left, InventoryItemId? right)
    {
        return !Equals(left, right);
    }
}
