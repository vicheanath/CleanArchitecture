using Clean.Architecture.Application.Orders;
using Clean.Architecture.Application.Orders.DTOs;
using Clean.Architecture.Application.Orders.CreateOrder;
using Clean.Architecture.Application.Orders.GetAllOrders;
using Clean.Architecture.Application.Orders.GetOrderById;
using Shared.Messaging;
using Shared.Results;
using Microsoft.AspNetCore.Mvc;

namespace Clean.Architecture.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IQueryHandler<GetAllOrdersQuery, IReadOnlyList<OrderDto>> _getAllOrdersHandler;
    private readonly IQueryHandler<GetOrderByIdQuery, OrderDto?> _getOrderByIdHandler;
    private readonly ICommandHandler<CreateOrderCommand, CreateOrderResult> _createOrderHandler;

    public OrdersController(
        IQueryHandler<GetAllOrdersQuery, IReadOnlyList<OrderDto>> getAllOrdersHandler,
        IQueryHandler<GetOrderByIdQuery, OrderDto?> getOrderByIdHandler,
        ICommandHandler<CreateOrderCommand, CreateOrderResult> createOrderHandler)
    {
        _getAllOrdersHandler = getAllOrdersHandler;
        _getOrderByIdHandler = getOrderByIdHandler;
        _createOrderHandler = createOrderHandler;
    }

    [HttpGet]
    public async Task<ActionResult<Result<IReadOnlyList<OrderDto>>>> GetAllOrders(CancellationToken cancellationToken)
    {
        var result = await _getAllOrdersHandler.Handle(new GetAllOrdersQuery(), cancellationToken);
        return result;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Result<OrderDto?>>> GetOrderById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getOrderByIdHandler.Handle(new GetOrderByIdQuery(id), cancellationToken);
        return result;
    }

    [HttpPost]
    public async Task<ActionResult<Result<CreateOrderResult>>> CreateOrder(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateOrderCommand(
            request.CustomerName,
            request.CustomerEmail,
            request.ShippingAddress,
            request.Items.Select(i => new OrderItemRequest(i.ProductSku, i.Quantity)).ToList());

        var result = await _createOrderHandler.Handle(command, cancellationToken);

        if (result.IsSuccess)
        {
            return CreatedAtAction(
                nameof(GetOrderById),
                new { id = result.Value.Id },
                result);
        }

        return result;
    }
}

public record CreateOrderRequest(string CustomerName, string CustomerEmail, string ShippingAddress, List<CreateOrderItemRequest> Items);

public record CreateOrderItemRequest(string ProductSku, int Quantity);
