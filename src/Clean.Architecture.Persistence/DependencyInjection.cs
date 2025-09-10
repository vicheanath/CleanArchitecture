using Clean.Architecture.Application.Orders;
using Clean.Architecture.Application.Products;
using Clean.Architecture.Domain.Inventory;
using Clean.Architecture.Persistence.Inventory;
using Clean.Architecture.Persistence.Orders;
using Clean.Architecture.Persistence.Products;
using Microsoft.Extensions.DependencyInjection;

namespace Clean.Architecture.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services.AddSingleton<IProductRepository, InMemoryProductRepository>();
        services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
        services.AddSingleton<IInventoryItemRepository, InMemoryInventoryItemRepository>();

        return services;
    }
}
