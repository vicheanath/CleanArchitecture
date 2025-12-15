using Clean.Architecture.Application.Inventory.ReserveInventory;
using Clean.Architecture.Domain.Inventory;
using Moq;
using Shared.Errors;
using Shared.Results;

namespace Clean.Architecture.Application.UnitTests.Inventory;

public class ReserveInventoryCommandHandlerTests
{
    private readonly Mock<IInventoryItemRepository> _inventoryItemRepositoryMock;
    private readonly ReserveInventoryCommandHandler _handler;

    public ReserveInventoryCommandHandlerTests()
    {
        _inventoryItemRepositoryMock = new Mock<IInventoryItemRepository>();
        _handler = new ReserveInventoryCommandHandler(_inventoryItemRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ReservesStock()
    {
        // Arrange
        var inventoryItemId = Guid.NewGuid();
        var inventoryItem = InventoryItem.Create("SKU-001", 100, 10);
        var initialAvailableQuantity = inventoryItem.AvailableQuantity;

        var command = new ReserveInventoryCommand(inventoryItemId, 20, "RES-001", DateTime.UtcNow.AddHours(24));

        _inventoryItemRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<InventoryItemId>(id => id.Value == inventoryItemId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventoryItem);

        _inventoryItemRepositoryMock
            .Setup(x => x.Update(It.IsAny<InventoryItem>()));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(20, inventoryItem.ReservedQuantity);
        Assert.Equal(initialAvailableQuantity - 20, inventoryItem.AvailableQuantity);

        _inventoryItemRepositoryMock.Verify(x => x.Update(It.IsAny<InventoryItem>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentInventoryItem_ReturnsFailure()
    {
        // Arrange
        var inventoryItemId = Guid.NewGuid();
        var command = new ReserveInventoryCommand(inventoryItemId, 20, "RES-001", null);

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
    public async Task Handle_WithZeroQuantity_ReturnsFailure()
    {
        // Arrange
        var inventoryItemId = Guid.NewGuid();
        var inventoryItem = InventoryItem.Create("SKU-001", 100, 10);

        var command = new ReserveInventoryCommand(inventoryItemId, 0, "RES-001", null);

        _inventoryItemRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<InventoryItemId>(id => id.Value == inventoryItemId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventoryItem);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Inventory.InvalidReservationQuantity", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WithEmptyReservationId_ReturnsFailure()
    {
        // Arrange
        var inventoryItemId = Guid.NewGuid();
        var inventoryItem = InventoryItem.Create("SKU-001", 100, 10);

        var command = new ReserveInventoryCommand(inventoryItemId, 20, "", null);

        _inventoryItemRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<InventoryItemId>(id => id.Value == inventoryItemId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventoryItem);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Inventory.ReservationIdRequired", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WithInsufficientAvailableStock_ReturnsFailure()
    {
        // Arrange
        var inventoryItemId = Guid.NewGuid();
        var inventoryItem = InventoryItem.Create("SKU-001", 100, 10);
        inventoryItem.ReserveStock(80, "RES-EXISTING"); // Reserve 80, leaving 20 available

        var command = new ReserveInventoryCommand(inventoryItemId, 30, "RES-NEW", null); // Try to reserve 30

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
