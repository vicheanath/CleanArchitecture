using Clean.Architecture.Application.Common.Interfaces;
using Clean.Architecture.Application.Orders;
using Clean.Architecture.Application.Products;
using Clean.Architecture.Domain.Inventory;
using Clean.Architecture.Domain.Users;
using Clean.Architecture.Persistence.Interceptors;
using Clean.Architecture.Persistence.Repositories;
using Clean.Architecture.Persistence.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Messaging;

namespace Clean.Architecture.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        // Register domain event publisher
        services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();

        // Register interceptors
        services.AddScoped<PublishDomainEventsInterceptor>();

        // Add Entity Framework Core with In-Memory database and interceptors
        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            var interceptor = serviceProvider.GetRequiredService<PublishDomainEventsInterceptor>();
            options.UseInMemoryDatabase("CleanArchitectureDb")
                   .AddInterceptors(interceptor);
        });

        // Register repositories
        services.AddScoped<IProductRepository, EfProductRepository>();
        services.AddScoped<IOrderRepository, EfOrderRepository>();
        services.AddScoped<IInventoryItemRepository, EfInventoryItemRepository>();
        services.AddScoped<IUserRepository, EfUserRepository>();
        services.AddScoped<IRoleRepository, EfRoleRepository>();

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
