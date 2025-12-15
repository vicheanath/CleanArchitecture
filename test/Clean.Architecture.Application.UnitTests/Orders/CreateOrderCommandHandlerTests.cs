using Clean.Architecture.Application.Orders;
using Clean.Architecture.Application.Orders.CreateOrder;
using Clean.Architecture.Application.Orders.DTOs;
using Clean.Architecture.Application.Products;
using Clean.Architecture.Domain.Orders;
using Clean.Architecture.Domain.Products;
using Moq;
using Shared.Errors;
using Shared.Results;

namespace Clean.Architecture.Application.UnitTests.Orders;

public class CreateOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _handler = new CreateOrderCommandHandler(_orderRepositoryMock.Object, _productRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_CreatesOrder()
    {
        // Arrange
        var product1 = Product.Create("SKU-001", "Product 1", "Description", 10.00m, "Category");
        var product2 = Product.Create("SKU-002", "Product 2", "Description", 20.00m, "Category");

        var command = new CreateOrderCommand(
            "John Doe",
            "john@example.com",
            "123 Main St",
            new List<OrderItemRequest>
            {
                new("SKU-001", 2),
                new("SKU-002", 1)
            });

        _productRepositoryMock
            .Setup(x => x.GetBySkuAsync("SKU-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(product1);

        _productRepositoryMock
            .Setup(x => x.GetBySkuAsync("SKU-002", It.IsAny<CancellationToken>()))
            .ReturnsAsync(product2);

        _orderRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("John Doe", result.Value.CustomerName);
        Assert.Equal("john@example.com", result.Value.CustomerEmail);
        Assert.Equal(40.00m, result.Value.TotalAmount); // (10 * 2) + (20 * 1)

        _orderRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentProductSku_ReturnsFailure()
    {
        // Arrange
        var command = new CreateOrderCommand(
            "John Doe",
            "john@example.com",
            "123 Main St",
            new List<OrderItemRequest> { new("SKU-999", 1) });

        _productRepositoryMock
            .Setup(x => x.GetBySkuAsync("SKU-999", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Order.ProductNotFound", result.Error.Code);

        _orderRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
