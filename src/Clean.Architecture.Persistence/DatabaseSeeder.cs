using Clean.Architecture.Domain.Products;
using Clean.Architecture.Domain.Inventory;
using Clean.Architecture.Domain.Orders;
using Clean.Architecture.Domain.Users;
using BCrypt.Net;

namespace Clean.Architecture.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Seed Roles and Users first
        await SeedRolesAndUsersAsync(context);

        if (context.Products.Any())
        {
            return; // Database has been seeded
        }

        // Seed Products
        var products = new[]
        {
            Product.Create("LAPTOP001", "Gaming Laptop", "High-performance gaming laptop with RTX 4080", 1299.99m, "Electronics"),
            Product.Create("MOUSE001", "Wireless Mouse", "Ergonomic wireless mouse with RGB lighting", 49.99m, "Electronics"),
            Product.Create("KEYBOARD001", "Mechanical Keyboard", "RGB mechanical keyboard with Cherry MX switches", 129.99m, "Electronics"),
            Product.Create("MONITOR001", "4K Monitor", "27-inch 4K monitor with HDR support", 349.99m, "Electronics"),
            Product.Create("HEADSET001", "Gaming Headset", "Surround sound gaming headset with noise cancellation", 89.99m, "Electronics")
        };

        foreach (var product in products)
        {
            await context.Products.AddAsync(product);
        }

        // Seed Inventory Items
        var inventoryItems = new[]
        {
            InventoryItem.Create("LAPTOP001", 10, 2),
            InventoryItem.Create("MOUSE001", 50, 10),
            InventoryItem.Create("KEYBOARD001", 25, 5),
            InventoryItem.Create("MONITOR001", 15, 3),
            InventoryItem.Create("HEADSET001", 30, 5)
        };

        foreach (var item in inventoryItems)
        {
            await context.InventoryItems.AddAsync(item);
        }

        // Seed Sample Orders
        var order1 = Order.Create("John Doe", "john.doe@example.com", "123 Main St, Anytown, AT 12345");
        order1.AddItem("LAPTOP001", "Gaming Laptop", 1299.99m, 1);
        order1.AddItem("MOUSE001", "Wireless Mouse", 49.99m, 1);
        await context.Orders.AddAsync(order1);

        var order2 = Order.Create("Jane Smith", "jane.smith@example.com", "456 Oak Ave, Somewhere, ST 67890");
        order2.AddItem("KEYBOARD001", "Mechanical Keyboard", 129.99m, 1);
        order2.AddItem("MONITOR001", "4K Monitor", 349.99m, 1);
        await context.Orders.AddAsync(order2);

        await context.SaveChangesAsync();
    }

    private static async Task SeedRolesAndUsersAsync(ApplicationDbContext context)
    {
        // Check if roles already exist
        if (context.Roles.Any())
        {
            return; // Roles and users have been seeded
        }

        // Create Admin Role with all permissions
        var adminRole = Role.Create("Admin", "Administrator with full access to all features");
        adminRole.AddPermission(Permission.ProductsRead);
        adminRole.AddPermission(Permission.ProductsWrite);
        adminRole.AddPermission(Permission.OrdersRead);
        adminRole.AddPermission(Permission.OrdersCreate);
        adminRole.AddPermission(Permission.OrdersManage);
        adminRole.AddPermission(Permission.InventoryRead);
        adminRole.AddPermission(Permission.InventoryWrite);
        await context.Roles.AddAsync(adminRole);

        // Create Manager Role with most permissions
        var managerRole = Role.Create("Manager", "Manager with read/write access to most features");
        managerRole.AddPermission(Permission.ProductsRead);
        managerRole.AddPermission(Permission.ProductsWrite);
        managerRole.AddPermission(Permission.OrdersRead);
        managerRole.AddPermission(Permission.OrdersCreate);
        managerRole.AddPermission(Permission.OrdersManage);
        managerRole.AddPermission(Permission.InventoryRead);
        managerRole.AddPermission(Permission.InventoryWrite);
        await context.Roles.AddAsync(managerRole);

        // Create User Role with read-only and basic create permissions
        var userRole = Role.Create("User", "Regular user with read access and ability to create orders");
        userRole.AddPermission(Permission.ProductsRead);
        userRole.AddPermission(Permission.OrdersRead);
        userRole.AddPermission(Permission.OrdersCreate);
        userRole.AddPermission(Permission.InventoryRead);
        await context.Roles.AddAsync(userRole);

        await context.SaveChangesAsync();

        // Create Admin User
        var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"); // Default password - should be changed in production
        var adminUser = User.Create("admin@example.com", adminPasswordHash, "Admin", "User");
        adminUser.AddRole(adminRole);
        await context.Users.AddAsync(adminUser);

        // Create Manager User
        var managerPasswordHash = BCrypt.Net.BCrypt.HashPassword("Manager123!"); // Default password
        var managerUser = User.Create("manager@example.com", managerPasswordHash, "Manager", "User");
        managerUser.AddRole(managerRole);
        await context.Users.AddAsync(managerUser);

        // Create Regular User
        var userPasswordHash = BCrypt.Net.BCrypt.HashPassword("User123!"); // Default password
        var regularUser = User.Create("user@example.com", userPasswordHash, "Regular", "User");
        regularUser.AddRole(userRole);
        await context.Users.AddAsync(regularUser);

        await context.SaveChangesAsync();
    }
}
