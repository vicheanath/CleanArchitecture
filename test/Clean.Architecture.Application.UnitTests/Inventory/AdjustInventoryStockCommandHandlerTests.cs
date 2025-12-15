using Clean.Architecture.Application.Inventory.AdjustInventoryStock;
using Clean.Architecture.Domain.Inventory;
using Moq;
using Shared.Errors;
using Shared.Results;

namespace Clean.Architecture.Application.UnitTests.Inventory;

public class AdjustInventoryStockCommandHandlerTests
{
    private readonly Mock<IInventoryItemRepository> _inventoryItemRepositoryMock;
    private readonly AdjustInventoryStockCommandHandler _handler;

    public AdjustInventoryStockCommandHandlerTests()
    {
        _inventoryItemRepositoryMock = new Mock<IInventoryItemRepository>();
        _handler = new AdjustInventoryStockCommandHandler(_inventoryItemRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithPositiveQuantityChange_IncreasesStock()
    {
        // Arrange
        var inventoryItemId = Guid.NewGuid();
        var inventoryItem = InventoryItem.Create("SKU-001", 100, 10);
        var initialQuantity = inventoryItem.Quantity;

        var command = new AdjustInventoryStockCommand(inventoryItemId, 50, "Restock");

        _inventoryItemRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<InventoryItemId>(id => id.Value == inventoryItemId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventoryItem);

        _inventoryItemRepositoryMock
            .Setup(x => x.Update(It.IsAny<InventoryItem>()));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(initialQuantity + 50, inventoryItem.Quantity);

        _inventoryItemRepositoryMock.Verify(x => x.Update(It.IsAny<InventoryItem>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNegativeQuantityChange_DecreasesStock()
    {
        // Arrange
        var inventoryItemId = Guid.NewGuid();
        var inventoryItem = InventoryItem.Create("SKU-001", 100, 10);
        var initialQuantity = inventoryItem.Quantity;

        var command = new AdjustInventoryStockCommand(inventoryItemId, -30, "Sale");

        _inventoryItemRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<InventoryItemId>(id => id.Value == inventoryItemId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventoryItem);

        _inventoryItemRepositoryMock
            .Setup(x => x.Update(It.IsAny<InventoryItem>()));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(initialQuantity - 30, inventoryItem.Quantity);
    }

    [Fact]
    public async Task Handle_WithZeroQuantityChange_ReturnsSuccess()
    {
        // Arrange
        var inventoryItemId = Guid.NewGuid();
        var inventoryItem = InventoryItem.Create("SKU-001", 100, 10);

        var command = new AdjustInventoryStockCommand(inventoryItemId, 0, "No change");

        _inventoryItemRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<InventoryItemId>(id => id.Value == inventoryItemId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventoryItem);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        _inventoryItemRepositoryMock.Verify(x => x.Update(It.IsAny<InventoryItem>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonExistentInventoryItem_ReturnsFailure()
    {
        // Arrange
        var inventoryItemId = Guid.NewGuid();
        var command = new AdjustInventoryStockCommand(inventoryItemId, 50, "Restock");

        _inventoryItemRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<InventoryItemId>(id => id.Value == inventoryItemId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((InventoryItem?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.IsType<NotFoundError>(result.Error);
        Assert.Equal("Inventory.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WithInsufficientStock_ReturnsFailure()
    {
        // Arrange
        var inventoryItemId = Guid.NewGuid();
        var inventoryItem = InventoryItem.Create("SKU-001", 100, 10);

        var command = new AdjustInventoryStockCommand(inventoryItemId, -150, "Sale"); // Try to remove more than available

        _inventoryItemRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<InventoryItemId>(id => id.Value == inventoryItemId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventoryItem);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Inventory.InsufficientStock", result.Error.Code);
    }
}
