using Clean.Architecture.Application.Products;
using Clean.Architecture.Application.Products.GetAllProducts;
using Clean.Architecture.Domain.Products;
using Moq;
using Shared.Results;

namespace Clean.Architecture.Application.UnitTests.Products;

public class GetAllProductsQueryHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly GetAllProductsQueryHandler _handler;

    public GetAllProductsQueryHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _handler = new GetAllProductsQueryHandler(_productRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithProducts_ReturnsAllProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            Product.Create("SKU-001", "Product 1", "Description 1", 10.00m, "Category"),
            Product.Create("SKU-002", "Product 2", "Description 2", 20.00m, "Category"),
            Product.Create("SKU-003", "Product 3", "Description 3", 30.00m, "Category")
        };

        _productRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        var query = new GetAllProductsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.Count);
        Assert.Equal("SKU-001", result.Value[0].Sku);
        Assert.Equal("SKU-002", result.Value[1].Sku);
        Assert.Equal("SKU-003", result.Value[2].Sku);
    }

    [Fact]
    public async Task Handle_WithNoProducts_ReturnsEmptyList()
    {
        // Arrange
        _productRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>());

        var query = new GetAllProductsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }
}
