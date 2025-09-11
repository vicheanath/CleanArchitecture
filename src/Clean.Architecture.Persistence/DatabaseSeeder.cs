using Clean.Architecture.Domain.Products;
using Clean.Architecture.Domain.Inventory;
using Clean.Architecture.Domain.Orders;

namespace Clean.Architecture.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
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
}
