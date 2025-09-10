using Shared.Primitives;

namespace Clean.Architecture.Domain.Inventory.Events;

/// <summary>
/// Domain event that is raised when an inventory item is created.
/// </summary>
public sealed record InventoryItemCreatedDomainEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    InventoryItemId InventoryItemId,
    string ProductSku,
    int InitialQuantity,
    int MinimumStockLevel) : DomainEvent(Id, OccurredOnUtc);

/// <summary>
/// Domain event that is raised when inventory stock is increased.
/// </summary>
public sealed record InventoryStockIncreasedDomainEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    InventoryItemId InventoryItemId,
    string ProductSku,
    int QuantityAdded,
    int PreviousQuantity,
    int NewQuantity,
    string? Reason) : DomainEvent(Id, OccurredOnUtc);

/// <summary>
/// Domain event that is raised when inventory stock is decreased.
/// </summary>
public sealed record InventoryStockDecreasedDomainEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    InventoryItemId InventoryItemId,
    string ProductSku,
    int QuantityRemoved,
    int PreviousQuantity,
    int NewQuantity,
    string? Reason) : DomainEvent(Id, OccurredOnUtc);

/// <summary>
/// Domain event that is raised when inventory reaches or falls below minimum stock level.
/// </summary>
public sealed record LowStockWarningDomainEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    InventoryItemId InventoryItemId,
    string ProductSku,
    int CurrentQuantity,
    int MinimumStockLevel) : DomainEvent(Id, OccurredOnUtc);

/// <summary>
/// Domain event that is raised when inventory is out of stock.
/// </summary>
public sealed record OutOfStockDomainEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    InventoryItemId InventoryItemId,
    string ProductSku) : DomainEvent(Id, OccurredOnUtc);

/// <summary>
/// Domain event that is raised when inventory minimum stock level is updated.
/// </summary>
public sealed record MinimumStockLevelUpdatedDomainEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    InventoryItemId InventoryItemId,
    string ProductSku,
    int PreviousMinimumStockLevel,
    int NewMinimumStockLevel) : DomainEvent(Id, OccurredOnUtc);

/// <summary>
/// Domain event that is raised when inventory is reserved for an order.
/// </summary>
public sealed record InventoryReservedDomainEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    InventoryItemId InventoryItemId,
    string ProductSku,
    int QuantityReserved,
    int AvailableQuantity,
    string ReservationId) : DomainEvent(Id, OccurredOnUtc);

/// <summary>
/// Domain event that is raised when inventory reservation is released.
/// </summary>
public sealed record InventoryReservationReleasedDomainEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    InventoryItemId InventoryItemId,
    string ProductSku,
    int QuantityReleased,
    int AvailableQuantity,
    string ReservationId) : DomainEvent(Id, OccurredOnUtc);
