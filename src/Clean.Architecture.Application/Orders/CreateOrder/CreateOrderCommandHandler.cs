using Clean.Architecture.Application.Orders.DTOs;
using Clean.Architecture.Application.Products;
using Clean.Architecture.Domain.Orders;
using Clean.Architecture.Domain.Products;
using Shared.Errors;
using Shared.Messaging;
using Shared.Results;

namespace Clean.Architecture.Application.Orders.CreateOrder;

/// <summary>
/// Handler for creating a new order
/// </summary>
public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, CreateOrderResult>
{
    private readonly IOrderRepository _orderRepository;
    private readonly Application.Products.IProductRepository _productRepository;

    public CreateOrderCommandHandler(IOrderRepository orderRepository, Application.Products.IProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
    }

    public async Task<Result<CreateOrderResult>> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var order = Order.Create(command.CustomerName, command.CustomerEmail, command.ShippingAddress);

        foreach (var itemRequest in command.Items)
        {
            var product = await _productRepository.GetBySkuAsync(itemRequest.ProductSku, cancellationToken);
            if (product == null)
                return Result.Failure<CreateOrderResult>(OrderErrors.ProductNotFoundForSku(itemRequest.ProductSku));

            order.AddItem(product.Sku, product.Name, product.ProductPricing.RegularPrice, itemRequest.Quantity);
        }

        await _orderRepository.AddAsync(order, cancellationToken);

        var result = new CreateOrderResult(
            order.Id.Value,
            order.CustomerName,
            order.CustomerEmail,
            order.TotalAmount,
            order.CreatedOnUtc);

        return Result.Success(result);
    }
}
