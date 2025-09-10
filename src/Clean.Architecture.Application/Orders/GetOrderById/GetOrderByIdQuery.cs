using Clean.Architecture.Application.Orders.DTOs;
using Shared.Messaging;

namespace Clean.Architecture.Application.Orders.GetOrderById;

/// <summary>
/// Query to get an order by its ID
/// </summary>
public record GetOrderByIdQuery(Guid Id) : IQuery<OrderDto?>;
