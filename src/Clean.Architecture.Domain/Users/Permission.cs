namespace Clean.Architecture.Domain.Users;

/// <summary>
/// Represents a permission in the system.
/// </summary>
public static class Permission
{
    // Products permissions
    public const string ProductsRead = "Products.Read";
    public const string ProductsWrite = "Products.Write";

    // Orders permissions
    public const string OrdersRead = "Orders.Read";
    public const string OrdersCreate = "Orders.Create";
    public const string OrdersManage = "Orders.Manage";

    // Inventory permissions
    public const string InventoryRead = "Inventory.Read";
    public const string InventoryWrite = "Inventory.Write";

    /// <summary>
    /// Gets all available permissions.
    /// </summary>
    public static IReadOnlyList<string> All => new[]
    {
        ProductsRead,
        ProductsWrite,
        OrdersRead,
        OrdersCreate,
        OrdersManage,
        InventoryRead,
        InventoryWrite
    };

    /// <summary>
    /// Validates if a permission string is valid.
    /// </summary>
    public static bool IsValid(string permission)
    {
        return All.Contains(permission);
    }
}
