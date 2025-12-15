using Clean.Architecture.Application.Common.Interfaces;
using Clean.Architecture.Application.Products;
using Clean.Architecture.Application.Products.CreateProduct;
using Clean.Architecture.Domain.Products;
using Moq;
using Shared.Errors;
using Shared.Results;

namespace Clean.Architecture.Application.UnitTests.Products;

public class CreateProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CreateProductCommandHandler(_productRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_CreatesProduct()
    {
        // Arrange
        var command = new CreateProductCommand(
            "SKU-001",
            "Test Product",
            "Description",
            10.00m,
            "Category",
            "Brand",
            5.5m,
            "12x8x1",
            "Red",
            "Large");

        _productRepositoryMock
            .Setup(x => x.GetBySkuAsync(command.Sku, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        _productRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("SKU-001", result.Value.Sku);
        Assert.Equal("Test Product", result.Value.Name);
        Assert.Equal(10.00m, result.Value.Price);

        _productRepositoryMock.Verify(x => x.GetBySkuAsync(command.Sku, It.IsAny<CancellationToken>()), Times.Once);
        _productRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDuplicateSku_ReturnsFailure()
    {
        // Arrange
        var command = new CreateProductCommand(
            "SKU-001",
            "Test Product",
            "Description",
            10.00m,
            "Category");

        var existingProduct = Product.Create("SKU-001", "Existing Product", "Description", 10.00m, "Category");

        _productRepositoryMock
            .Setup(x => x.GetBySkuAsync(command.Sku, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.IsType<ConflictError>(result.Error);
        Assert.Equal("Product.DuplicateSku", result.Error.Code);

        _productRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithSalePrice_CreatesProductWithSale()
    {
        // Arrange
        var command = new CreateProductCommand(
            "SKU-001",
            "Test Product",
            "Description",
            10.00m,
            "Category",
            SalePrice: 5.00m,
            SaleStartDate: DateTime.UtcNow.AddHours(-1),
            SaleEndDate: DateTime.UtcNow.AddHours(1));

        _productRepositoryMock
            .Setup(x => x.GetBySkuAsync(command.Sku, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        _productRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(5.00m, result.Value.SalePrice);
    }

    [Fact]
    public async Task Handle_WithImagesAndTags_CreatesProductWithImagesAndTags()
    {
        // Arrange
        var command = new CreateProductCommand(
            "SKU-001",
            "Test Product",
            "Description",
            10.00m,
            "Category",
            Images: new List<string> { "https://example.com/image1.jpg", "https://example.com/image2.jpg" },
            Tags: new List<string> { "electronics", "smartphone" });

        _productRepositoryMock
            .Setup(x => x.GetBySkuAsync(command.Sku, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        _productRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Images.Count);
        Assert.Equal(2, result.Value.Tags.Count);
    }
}
