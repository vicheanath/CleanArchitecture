using Clean.Architecture.Application.Orders.DTOs;
using Clean.Architecture.Domain.Orders;
using Shared.Errors;
using Shared.Messaging;
using Shared.Results;

namespace Clean.Architecture.Application.Orders.GetOrderById;

/// <summary>
/// Handler for getting an order by its ID
/// </summary>
public class GetOrderByIdQueryHandler : IQueryHandler<GetOrderByIdQuery, OrderDto?>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderByIdQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<OrderDto?>> Handle(GetOrderByIdQuery query, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(OrderId.Create(query.Id), cancellationToken);

        if (order == null)
        {
            return Result.Failure<OrderDto?>(OrderErrors.NotFoundWithId(query.Id));
        }

        var orderDto = new OrderDto(
            order.Id.Value,
            order.CustomerName,
            order.CustomerEmail,
            order.ShippingAddress,
            order.Status,
            order.TotalAmount,
            order.OrderDate,
            order.CreatedOnUtc,
            order.Items.Select(i => new OrderItemDto(
                i.ProductSku,
                i.ProductName,
                i.UnitPrice,
                i.Quantity,
                i.TotalPrice)).ToList());

        return Result.Success<OrderDto?>(orderDto);
    }
}
