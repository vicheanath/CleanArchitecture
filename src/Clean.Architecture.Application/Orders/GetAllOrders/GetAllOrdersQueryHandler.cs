using Clean.Architecture.Application.Orders.DTOs;
using Shared.Messaging;
using Shared.Results;

namespace Clean.Architecture.Application.Orders.GetAllOrders;

/// <summary>
/// Handler for getting all orders
/// </summary>
public class GetAllOrdersQueryHandler : IQueryHandler<GetAllOrdersQuery, IReadOnlyList<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;

    public GetAllOrdersQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<IReadOnlyList<OrderDto>>> Handle(GetAllOrdersQuery query, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetAllAsync(cancellationToken);

        var orderDtos = orders.Select(o => new OrderDto(
            o.Id.Value,
            o.CustomerName,
            o.CustomerEmail,
            o.ShippingAddress,
            o.Status,
            o.TotalAmount,
            o.OrderDate,
            o.CreatedOnUtc,
            o.Items.Select(i => new OrderItemDto(
                i.ProductSku,
                i.ProductName,
                i.UnitPrice,
                i.Quantity,
                i.TotalPrice)).ToList())).ToList();

        return Result.Success<IReadOnlyList<OrderDto>>(orderDtos);
    }
}
