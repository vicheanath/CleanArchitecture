using Clean.Architecture.Application.Orders;
using Clean.Architecture.Application.Orders.GetOrderById;
using Clean.Architecture.Domain.Orders;
using Moq;
using Shared.Results;

namespace Clean.Architecture.Application.UnitTests.Orders;

public class GetOrderByIdQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly GetOrderByIdQueryHandler _handler;

    public GetOrderByIdQueryHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _handler = new GetOrderByIdQueryHandler(_orderRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingOrderId_ReturnsOrderDto()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = Order.Create("John Doe", "john@example.com", "123 Main St");
        order.AddItem("SKU-001", "Product 1", 10.00m, 2);

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<OrderId>(o => o.Value == orderId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var query = new GetOrderByIdQuery(orderId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("John Doe", result.Value!.CustomerName);
        Assert.Equal("john@example.com", result.Value.CustomerEmail);
        Assert.Equal(20.00m, result.Value.TotalAmount);
        Assert.Single(result.Value.Items);
    }

    [Fact]
    public async Task Handle_WithNonExistentOrderId_ReturnsFailure()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<OrderId>(o => o.Value == orderId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var query = new GetOrderByIdQuery(orderId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Order.NotFound", result.Error.Code);
    }
}
