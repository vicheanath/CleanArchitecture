using Clean.Architecture.Application.Orders.DTOs;
using Shared.Messaging;

namespace Clean.Architecture.Application.Orders.CreateOrder;

/// <summary>
/// Command to create a new order
/// </summary>
public record CreateOrderCommand(
    string CustomerName,
    string CustomerEmail,
    string ShippingAddress,
    List<OrderItemRequest> Items) : ICommand<CreateOrderResult>;
