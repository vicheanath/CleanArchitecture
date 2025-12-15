using Clean.Architecture.Application.Inventory.CreateInventoryItem;
using Clean.Architecture.Domain.Inventory;
using Moq;
using Shared.Errors;
using Shared.Results;

namespace Clean.Architecture.Application.UnitTests.Inventory;

public class CreateInventoryItemCommandHandlerTests
{
    private readonly Mock<IInventoryItemRepository> _inventoryItemRepositoryMock;
    private readonly CreateInventoryItemCommandHandler _handler;

    public CreateInventoryItemCommandHandlerTests()
    {
        _inventoryItemRepositoryMock = new Mock<IInventoryItemRepository>();
        _handler = new CreateInventoryItemCommandHandler(_inventoryItemRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_CreatesInventoryItem()
    {
        // Arrange
        var command = new CreateInventoryItemCommand("SKU-001", 100, 10);

        _inventoryItemRepositoryMock
            .Setup(x => x.GetByProductSkuAsync("SKU-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync((InventoryItem?)null);

        _inventoryItemRepositoryMock
            .Setup(x => x.Add(It.IsAny<InventoryItem>()));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        _inventoryItemRepositoryMock.Verify(x => x.GetByProductSkuAsync("SKU-001", It.IsAny<CancellationToken>()), Times.Once);
        _inventoryItemRepositoryMock.Verify(x => x.Add(It.IsAny<InventoryItem>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDuplicateProductSku_ReturnsFailure()
    {
        // Arrange
        var command = new CreateInventoryItemCommand("SKU-001", 100, 10);
        var existingItem = InventoryItem.Create("SKU-001", 50, 5);

        _inventoryItemRepositoryMock
            .Setup(x => x.GetByProductSkuAsync("SKU-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingItem);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.IsType<ConflictError>(result.Error);
        Assert.Equal("Inventory.DuplicateProductSku", result.Error.Code);

        _inventoryItemRepositoryMock.Verify(x => x.Add(It.IsAny<InventoryItem>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithEmptyProductSku_ReturnsFailure()
    {
        // Arrange
        var command = new CreateInventoryItemCommand("", 100, 10);

        _inventoryItemRepositoryMock
            .Setup(x => x.GetByProductSkuAsync("", It.IsAny<CancellationToken>()))
            .ReturnsAsync((InventoryItem?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Inventory.ProductSkuRequired", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WithNegativeQuantity_ReturnsFailure()
    {
        // Arrange
        var command = new CreateInventoryItemCommand("SKU-001", -10, 10);

        _inventoryItemRepositoryMock
            .Setup(x => x.GetByProductSkuAsync("SKU-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync((InventoryItem?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Inventory.InvalidQuantity", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WithNegativeMinimumStockLevel_ReturnsFailure()
    {
        // Arrange
        var command = new CreateInventoryItemCommand("SKU-001", 100, -10);

        _inventoryItemRepositoryMock
            .Setup(x => x.GetByProductSkuAsync("SKU-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync((InventoryItem?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Inventory.InvalidMinimumStockLevel", result.Error.Code);
    }
}
