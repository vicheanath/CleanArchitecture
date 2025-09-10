using Clean.Architecture.Domain.Orders;

namespace Clean.Architecture.Application.Orders.DTOs;

/// <summary>
/// Data Transfer Objects for Order operations
/// </summary>
public record OrderItemRequest(string ProductSku, int Quantity);

public record CreateOrderResult(
    Guid Id,
    string CustomerName,
    string CustomerEmail,
    decimal TotalAmount,
    DateTime CreatedOnUtc);

public record OrderDto(
    Guid Id,
    string CustomerName,
    string CustomerEmail,
    string ShippingAddress,
    OrderStatus Status,
    decimal TotalAmount,
    DateTime OrderDate,
    DateTime CreatedOnUtc,
    List<OrderItemDto> Items);

public record OrderItemDto(
    string ProductSku,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice);
