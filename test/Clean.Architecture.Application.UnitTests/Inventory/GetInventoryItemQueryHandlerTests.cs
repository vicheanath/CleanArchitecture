using Clean.Architecture.Application.Inventory.GetInventoryItem;
using Clean.Architecture.Domain.Inventory;
using Moq;
using Shared.Results;

namespace Clean.Architecture.Application.UnitTests.Inventory;

public class GetInventoryItemQueryHandlerTests
{
    private readonly Mock<IInventoryItemRepository> _inventoryItemRepositoryMock;
    private readonly GetInventoryItemQueryHandler _handler;

    public GetInventoryItemQueryHandlerTests()
    {
        _inventoryItemRepositoryMock = new Mock<IInventoryItemRepository>();
        _handler = new GetInventoryItemQueryHandler(_inventoryItemRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingInventoryItemId_ReturnsInventoryItemResponse()
    {
        // Arrange
        var inventoryItemId = Guid.NewGuid();
        var inventoryItem = InventoryItem.Create("SKU-001", 100, 10);

        var query = new GetInventoryItemQuery(inventoryItemId);

        _inventoryItemRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<InventoryItemId>(id => id.Value == inventoryItemId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventoryItem);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("SKU-001", result.Value.ProductSku);
        Assert.Equal(100, result.Value.Quantity);
        Assert.Equal(10, result.Value.MinimumStockLevel);
        Assert.False(result.Value.IsOutOfStock);
        Assert.False(result.Value.IsBelowMinimumStock);
    }

    [Fact]
    public async Task Handle_WithNonExistentInventoryItemId_ReturnsFailure()
    {
        // Arrange
        var inventoryItemId = Guid.NewGuid();
        var query = new GetInventoryItemQuery(inventoryItemId);

        _inventoryItemRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<InventoryItemId>(id => id.Value == inventoryItemId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((InventoryItem?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Inventory.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WithReservations_ReturnsCorrectReservedQuantity()
    {
        // Arrange
        var inventoryItemId = Guid.NewGuid();
        var inventoryItem = InventoryItem.Create("SKU-001", 100, 10);
        inventoryItem.ReserveStock(20, "RES-001");
        inventoryItem.ReserveStock(30, "RES-002");

        var query = new GetInventoryItemQuery(inventoryItemId);

        _inventoryItemRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<InventoryItemId>(id => id.Value == inventoryItemId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventoryItem);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(50, result.Value.ReservedQuantity);
        Assert.Equal(50, result.Value.AvailableQuantity);
    }
}
