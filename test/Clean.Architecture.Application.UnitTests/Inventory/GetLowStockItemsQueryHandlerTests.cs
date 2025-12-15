using Clean.Architecture.Application.Inventory.GetLowStockItems;
using Clean.Architecture.Domain.Inventory;
using Moq;
using Shared.Results;

namespace Clean.Architecture.Application.UnitTests.Inventory;

public class GetLowStockItemsQueryHandlerTests
{
    private readonly Mock<IInventoryItemRepository> _inventoryItemRepositoryMock;
    private readonly GetLowStockItemsQueryHandler _handler;

    public GetLowStockItemsQueryHandlerTests()
    {
        _inventoryItemRepositoryMock = new Mock<IInventoryItemRepository>();
        _handler = new GetLowStockItemsQueryHandler(_inventoryItemRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithLowStockItems_ReturnsLowStockItems()
    {
        // Arrange
        var lowStockItems = new List<InventoryItem>
        {
            InventoryItem.Create("SKU-001", 5, 10),  // Below minimum
            InventoryItem.Create("SKU-002", 3, 10),  // Below minimum
            InventoryItem.Create("SKU-003", 15, 10)  // Above minimum (should not be included)
        };

        _inventoryItemRepositoryMock
            .Setup(x => x.GetItemsBelowMinimumStockAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lowStockItems.Where(i => i.IsBelowMinimumStock).ToList());

        var query = new GetLowStockItemsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count());
        Assert.Equal("SKU-001", result.Value.First().ProductSku);
    }

    [Fact]
    public async Task Handle_WithNoLowStockItems_ReturnsEmptyList()
    {
        // Arrange
        _inventoryItemRepositoryMock
            .Setup(x => x.GetItemsBelowMinimumStockAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<InventoryItem>());

        var query = new GetLowStockItemsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task Handle_CalculatesCorrectShortageQuantity()
    {
        // Arrange
        var lowStockItem = InventoryItem.Create("SKU-001", 5, 10); // 5 short of minimum
        var lowStockItems = new List<InventoryItem> { lowStockItem };

        _inventoryItemRepositoryMock
            .Setup(x => x.GetItemsBelowMinimumStockAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lowStockItems);

        var query = new GetLowStockItemsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var response = result.Value.First();
        Assert.Equal(5, response.StockDeficit); // 10 (minimum) - 5 (current) = 5
    }
}
