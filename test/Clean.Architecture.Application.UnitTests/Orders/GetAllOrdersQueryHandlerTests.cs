using Clean.Architecture.Application.Orders;
using Clean.Architecture.Application.Orders.GetAllOrders;
using Clean.Architecture.Domain.Orders;
using Moq;
using Shared.Results;

namespace Clean.Architecture.Application.UnitTests.Orders;

public class GetAllOrdersQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly GetAllOrdersQueryHandler _handler;

    public GetAllOrdersQueryHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _handler = new GetAllOrdersQueryHandler(_orderRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithOrders_ReturnsAllOrders()
    {
        // Arrange
        var orders = new List<Order>
        {
            Order.Create("John Doe", "john@example.com", "123 Main St"),
            Order.Create("Jane Smith", "jane@example.com", "456 Oak Ave"),
            Order.Create("Bob Johnson", "bob@example.com", "789 Pine Rd")
        };

        orders[0].AddItem("SKU-001", "Product 1", 10.00m, 2);
        orders[1].AddItem("SKU-002", "Product 2", 20.00m, 1);
        orders[2].AddItem("SKU-003", "Product 3", 30.00m, 3);

        _orderRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        var query = new GetAllOrdersQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.Count);
        Assert.Equal("John Doe", result.Value[0].CustomerName);
        Assert.Equal("Jane Smith", result.Value[1].CustomerName);
        Assert.Equal("Bob Johnson", result.Value[2].CustomerName);
    }

    [Fact]
    public async Task Handle_WithNoOrders_ReturnsEmptyList()
    {
        // Arrange
        _orderRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Order>());

        var query = new GetAllOrdersQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }
}
