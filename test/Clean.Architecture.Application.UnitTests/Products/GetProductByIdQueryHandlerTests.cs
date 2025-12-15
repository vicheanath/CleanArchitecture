using Clean.Architecture.Application.Products;
using Clean.Architecture.Application.Products.GetProductById;
using Clean.Architecture.Domain.Products;
using Moq;
using Shared.Results;

namespace Clean.Architecture.Application.UnitTests.Products;

public class GetProductByIdQueryHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly GetProductByIdQueryHandler _handler;

    public GetProductByIdQueryHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _handler = new GetProductByIdQueryHandler(_productRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingProductId_ReturnsProductDto()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = Product.Create("SKU-001", "Test Product", "Description", 10.00m, "Category");

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<ProductId>(p => p.Value == productId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var query = new GetProductByIdQuery(productId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("SKU-001", result.Value!.Sku);
        Assert.Equal("Test Product", result.Value.Name);
        Assert.Equal(10.00m, result.Value.Price);
    }

    [Fact]
    public async Task Handle_WithNonExistentProductId_ReturnsFailure()
    {
        // Arrange
        var productId = Guid.NewGuid();

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<ProductId>(p => p.Value == productId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var query = new GetProductByIdQuery(productId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Product.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WithProductOnSale_ReturnsCorrectEffectivePrice()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = Product.Create(
            "SKU-001",
            "Test Product",
            "Description",
            10.00m,
            "Category",
            salePrice: 5.00m,
            saleStartDate: DateTime.UtcNow.AddHours(-1),
            saleEndDate: DateTime.UtcNow.AddHours(1));

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<ProductId>(p => p.Value == productId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var query = new GetProductByIdQuery(productId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(5.00m, result.Value!.EffectivePrice);
        Assert.True(result.Value.IsOnSale);
    }
}
