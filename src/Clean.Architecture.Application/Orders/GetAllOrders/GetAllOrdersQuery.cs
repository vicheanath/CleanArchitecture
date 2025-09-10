using Clean.Architecture.Application.Orders.DTOs;
using Shared.Messaging;

namespace Clean.Architecture.Application.Orders.GetAllOrders;

/// <summary>
/// Query to get all orders
/// </summary>
public record GetAllOrdersQuery() : IQuery<IReadOnlyList<OrderDto>>;
