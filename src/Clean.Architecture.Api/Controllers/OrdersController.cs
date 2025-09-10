using Clean.Architecture.Application.Orders;
using Clean.Architecture.Application.Orders.DTOs;
using Clean.Architecture.Application.Orders.CreateOrder;
using Clean.Architecture.Application.Orders.GetAllOrders;
using Clean.Architecture.Application.Orders.GetOrderById;
using Shared.Messaging;
using Microsoft.AspNetCore.Mvc;

namespace Clean.Architecture.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public OrdersController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetAllOrders(CancellationToken cancellationToken)
    {
        var orders = await _dispatcher.QueryAsync<GetAllOrdersQuery, IReadOnlyList<OrderDto>>(new GetAllOrdersQuery(), cancellationToken);
        return Ok(orders);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetOrderById(Guid id, CancellationToken cancellationToken)
    {
        var order = await _dispatcher.QueryAsync<GetOrderByIdQuery, OrderDto?>(new GetOrderByIdQuery(id), cancellationToken);
        return Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<CreateOrderResult>> CreateOrder(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateOrderCommand(
            request.CustomerName,
            request.CustomerEmail,
            request.ShippingAddress,
            request.Items.Select(i => new OrderItemRequest(i.ProductSku, i.Quantity)).ToList());

        var result = await _dispatcher.CommandAsync<CreateOrderCommand, CreateOrderResult>(command, cancellationToken);

        return CreatedAtAction(
            nameof(GetOrderById),
            new { id = result.Id },
            result);
    }
}

public record CreateOrderRequest(string CustomerName, string CustomerEmail, string ShippingAddress, List<CreateOrderItemRequest> Items);

public record CreateOrderItemRequest(string ProductSku, int Quantity);
