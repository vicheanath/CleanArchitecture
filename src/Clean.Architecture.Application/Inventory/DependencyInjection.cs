using Clean.Architecture.Application.Inventory.AdjustInventoryStock;
using Clean.Architecture.Application.Inventory.CreateInventoryItem;
using Clean.Architecture.Application.Inventory.ReserveInventory;
using Clean.Architecture.Application.Inventory.GetInventoryItem;
using Clean.Architecture.Application.Inventory.GetLowStockItems;
using Clean.Architecture.Application.Inventory.EventHandlers;
using Clean.Architecture.Domain.Inventory;
using Clean.Architecture.Domain.Inventory.Events;
using Microsoft.Extensions.DependencyInjection;
using Shared.Messaging;

namespace Clean.Architecture.Application.Inventory;

/// <summary>
/// Dependency injection extensions for Inventory feature.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all Inventory feature services.
    /// </summary>
    public static IServiceCollection AddInventory(this IServiceCollection services)
    {
        // Register Command Handlers
        services.AddScoped<ICommandHandler<CreateInventoryItemCommand, Guid>, CreateInventoryItemCommandHandler>();
        services.AddScoped<ICommandHandler<AdjustInventoryStockCommand>, AdjustInventoryStockCommandHandler>();
        services.AddScoped<ICommandHandler<ReserveInventoryCommand>, ReserveInventoryCommandHandler>();

        // Register Query Handlers
        services.AddScoped<IQueryHandler<GetInventoryItemQuery, GetInventoryItem.InventoryItemResponse>, GetInventoryItemQueryHandler>();
        services.AddScoped<IQueryHandler<GetLowStockItemsQuery, IEnumerable<GetLowStockItems.LowStockItemResponse>>, GetLowStockItemsQueryHandler>();

        // Register Domain Event Handlers
        services.AddScoped<IDomainEventHandler<InventoryItemCreatedDomainEvent>, InventoryItemCreatedDomainEventHandler>();
        services.AddScoped<IDomainEventHandler<InventoryStockIncreasedDomainEvent>, InventoryStockChangedDomainEventHandler>();
        services.AddScoped<IDomainEventHandler<InventoryStockDecreasedDomainEvent>, InventoryStockChangedDomainEventHandler>();
        services.AddScoped<IDomainEventHandler<LowStockWarningDomainEvent>, LowStockWarningDomainEventHandler>();
        services.AddScoped<IDomainEventHandler<OutOfStockDomainEvent>, OutOfStockDomainEventHandler>();
        services.AddScoped<IDomainEventHandler<InventoryReservedDomainEvent>, InventoryReservationDomainEventHandler>();
        services.AddScoped<IDomainEventHandler<InventoryReservationReleasedDomainEvent>, InventoryReservationDomainEventHandler>();

        return services;
    }
}
