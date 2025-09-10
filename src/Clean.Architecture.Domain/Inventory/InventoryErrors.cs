using Shared.Errors;

namespace Clean.Architecture.Domain.Inventory;

/// <summary>
/// Contains the inventory errors.
/// </summary>
public static class InventoryErrors
{
    /// <summary>
    /// Gets the inventory item not found error.
    /// </summary>
    public static NotFoundError NotFound => new(
        "Inventory.NotFound",
        "The inventory item with the specified identifier was not found");

    /// <summary>
    /// Gets the product SKU required error.
    /// </summary>
    public static Error ProductSkuRequired => new(
        "Inventory.ProductSkuRequired",
        "The product SKU is required");

    /// <summary>
    /// Gets the invalid quantity error.
    /// </summary>
    public static Error InvalidQuantity => new(
        "Inventory.InvalidQuantity",
        "The quantity must be greater than or equal to zero");

    /// <summary>
    /// Gets the insufficient stock error.
    /// </summary>
    public static Error InsufficientStock => new(
        "Inventory.InsufficientStock",
        "There is insufficient stock available for this operation");

    /// <summary>
    /// Gets the invalid minimum stock level error.
    /// </summary>
    public static Error InvalidMinimumStockLevel => new(
        "Inventory.InvalidMinimumStockLevel",
        "The minimum stock level must be greater than or equal to zero");

    /// <summary>
    /// Gets the reservation not found error.
    /// </summary>
    public static NotFoundError ReservationNotFound => new(
        "Inventory.ReservationNotFound",
        "The inventory reservation with the specified identifier was not found");

    /// <summary>
    /// Gets the invalid reservation quantity error.
    /// </summary>
    public static Error InvalidReservationQuantity => new(
        "Inventory.InvalidReservationQuantity",
        "The reservation quantity must be greater than zero");

    /// <summary>
    /// Gets the duplicate product SKU error.
    /// </summary>
    public static ConflictError DuplicateProductSku => new(
        "Inventory.DuplicateProductSku",
        "An inventory item with the specified product SKU already exists");
}
