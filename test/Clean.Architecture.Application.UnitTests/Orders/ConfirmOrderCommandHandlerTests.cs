using Clean.Architecture.Application.Orders;
using Clean.Architecture.Application.Orders.ConfirmOrder;
using Clean.Architecture.Domain.Inventory;
using Clean.Architecture.Domain.Orders;
using Moq;
using Shared.Errors;
using Shared.Results;

namespace Clean.Architecture.Application.UnitTests.Orders;

public class ConfirmOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IInventoryItemRepository> _inventoryRepositoryMock;
    private readonly ConfirmOrderCommandHandler _handler;

    public ConfirmOrderCommandHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _inventoryRepositoryMock = new Mock<IInventoryItemRepository>();
        _handler = new ConfirmOrderCommandHandler(_orderRepositoryMock.Object, _inventoryRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidOrder_ConfirmsOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = Order.Create("John Doe", "john@example.com", "123 Main St");
        order.AddItem("SKU-001", "Product 1", 10.00m, 2);

        var inventoryItem = InventoryItem.Create("SKU-001", 100, 10);

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<OrderId>(o => o.Value == orderId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _inventoryRepositoryMock
            .Setup(x => x.GetByProductSkuAsync("SKU-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventoryItem);

        _orderRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new ConfirmOrderCommand(orderId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatus.Confirmed, order.Status);

        _orderRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentOrder_ReturnsFailure()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<OrderId>(o => o.Value == orderId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var command = new ConfirmOrderCommand(orderId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.IsType<NotFoundError>(result.Error);
        Assert.Equal("Order.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WithOrderNotPending_ReturnsFailure()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = Order.Create("John Doe", "john@example.com", "123 Main St");
        order.AddItem("SKU-001", "Product 1", 10.00m, 2);
        order.ConfirmOrder(); // Already confirmed

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<OrderId>(o => o.Value == orderId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var command = new ConfirmOrderCommand(orderId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.IsType<ConflictError>(result.Error);
        Assert.Equal("Order.InvalidStatus", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WithInsufficientInventory_ReturnsFailure()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = Order.Create("John Doe", "john@example.com", "123 Main St");
        order.AddItem("SKU-001", "Product 1", 10.00m, 100); // Request 100 items

        var inventoryItem = InventoryItem.Create("SKU-001", 50, 10); // Only 50 available

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<OrderId>(o => o.Value == orderId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _inventoryRepositoryMock
            .Setup(x => x.GetByProductSkuAsync("SKU-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventoryItem);

        var command = new ConfirmOrderCommand(orderId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("OrderConfirmation.ValidationFailed", result.Error.Code);
    }
}
