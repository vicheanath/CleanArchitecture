using Shared.Primitives;

namespace Clean.Architecture.Domain.Orders;

/// <summary>
/// Represents a summary of an order item for domain events.
/// </summary>
/// <param name="ProductSku">The product SKU.</param>
/// <param name="Quantity">The quantity.</param>
public sealed record OrderItemSummary(string ProductSku, int Quantity);

/// <summary>
/// Domain event raised when an order is created.
/// </summary>
/// <param name="Id">The event identifier.</param>
/// <param name="OccurredOnUtc">The date and time the event occurred.</param>
/// <param name="OrderId">The order identifier.</param>
/// <param name="CustomerEmail">The customer email.</param>
public sealed record OrderCreatedDomainEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    OrderId OrderId,
    string CustomerEmail) : DomainEvent(Id, OccurredOnUtc);

/// <summary>
/// Domain event raised when an item is added to an order.
/// </summary>
/// <param name="Id">The event identifier.</param>
/// <param name="OccurredOnUtc">The date and time the event occurred.</param>
/// <param name="OrderId">The order identifier.</param>
/// <param name="ProductSku">The product SKU.</param>
/// <param name="Quantity">The quantity added.</param>
public sealed record OrderItemAddedDomainEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    OrderId OrderId,
    string ProductSku,
    int Quantity) : DomainEvent(Id, OccurredOnUtc);

/// <summary>
/// Domain event raised when an item is removed from an order.
/// </summary>
/// <param name="Id">The event identifier.</param>
/// <param name="OccurredOnUtc">The date and time the event occurred.</param>
/// <param name="OrderId">The order identifier.</param>
/// <param name="ProductSku">The product SKU.</param>
public sealed record OrderItemRemovedDomainEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    OrderId OrderId,
    string ProductSku) : DomainEvent(Id, OccurredOnUtc);

/// <summary>
/// Domain event raised when an order is confirmed.
/// </summary>
/// <param name="Id">The event identifier.</param>
/// <param name="OccurredOnUtc">The date and time the event occurred.</param>
/// <param name="OrderId">The order identifier.</param>
/// <param name="TotalAmount">The total amount of the order.</param>
/// <param name="Items">The order items.</param>
public sealed record OrderConfirmedDomainEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    OrderId OrderId,
    decimal TotalAmount,
    IReadOnlyList<OrderItemSummary> Items) : DomainEvent(Id, OccurredOnUtc);

/// <summary>
/// Domain event raised when an order is shipped.
/// </summary>
/// <param name="Id">The event identifier.</param>
/// <param name="OccurredOnUtc">The date and time the event occurred.</param>
/// <param name="OrderId">The order identifier.</param>
/// <param name="CustomerEmail">The customer email.</param>
/// <param name="ShippingAddress">The shipping address.</param>
public sealed record OrderShippedDomainEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    OrderId OrderId,
    string CustomerEmail,
    string ShippingAddress) : DomainEvent(Id, OccurredOnUtc);

/// <summary>
/// Domain event raised when an order is delivered.
/// </summary>
/// <param name="Id">The event identifier.</param>
/// <param name="OccurredOnUtc">The date and time the event occurred.</param>
/// <param name="OrderId">The order identifier.</param>
/// <param name="CustomerEmail">The customer email.</param>
public sealed record OrderDeliveredDomainEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    OrderId OrderId,
    string CustomerEmail) : DomainEvent(Id, OccurredOnUtc);

/// <summary>
/// Domain event raised when an order is cancelled.
/// </summary>
/// <param name="Id">The event identifier.</param>
/// <param name="OccurredOnUtc">The date and time the event occurred.</param>
/// <param name="OrderId">The order identifier.</param>
/// <param name="PreviousStatus">The previous order status before cancellation.</param>
/// <param name="Items">The order items that need inventory reservation release.</param>
public sealed record OrderCancelledDomainEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    OrderId OrderId,
    OrderStatus PreviousStatus,
    IReadOnlyList<OrderItemSummary> Items) : DomainEvent(Id, OccurredOnUtc);
