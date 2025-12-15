using Clean.Architecture.Application.Orders.CreateOrder;
using Clean.Architecture.Application.Orders.GetAllOrders;
using Clean.Architecture.Application.Orders.GetOrderById;
using Clean.Architecture.Application.Orders.DTOs;
using Clean.Architecture.Application.Orders.EventHandlers;
using Clean.Architecture.Domain.Orders;
using Microsoft.Extensions.DependencyInjection;
using Shared.Messaging;

namespace Clean.Architecture.Application.Orders;

/// <summary>
/// Dependency injection extensions for Orders feature.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all Orders feature services.
    /// </summary>
    public static IServiceCollection AddOrders(this IServiceCollection services)
    {
        // Register Command Handlers
        services.AddScoped<ICommandHandler<CreateOrderCommand, CreateOrderResult>, CreateOrderCommandHandler>();

        // Register Query Handlers
        services.AddScoped<IQueryHandler<GetAllOrdersQuery, IReadOnlyList<OrderDto>>, GetAllOrdersQueryHandler>();
        services.AddScoped<IQueryHandler<GetOrderByIdQuery, OrderDto?>, GetOrderByIdQueryHandler>();

        // Register Domain Event Handlers
        services.AddScoped<IDomainEventHandler<OrderConfirmedDomainEvent>, OrderConfirmedDomainEventHandler>();
        services.AddScoped<IDomainEventHandler<OrderShippedDomainEvent>, OrderShippedDomainEventHandler>();
        services.AddScoped<IDomainEventHandler<OrderCancelledDomainEvent>, OrderCancelledDomainEventHandler>();

        return services;
    }
}
