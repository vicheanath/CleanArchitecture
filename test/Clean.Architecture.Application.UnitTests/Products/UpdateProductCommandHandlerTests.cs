using Clean.Architecture.Application.Common.Interfaces;
using Clean.Architecture.Application.Products;
using Clean.Architecture.Application.Products.UpdateProduct;
using Clean.Architecture.Domain.Products;
using Moq;
using Shared.Errors;
using Shared.Results;

namespace Clean.Architecture.Application.UnitTests.Products;

public class UpdateProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateProductCommandHandler _handler;

    public UpdateProductCommandHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new UpdateProductCommandHandler(_productRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_UpdatesProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = Product.Create("SKU-001", "Old Name", "Old Description", 10.00m, "Category");

        var command = new UpdateProductCommand(
            productId,
            "New Name",
            "New Description",
            15.00m,
            "New Category",
            "New Brand");

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<ProductId>(p => p.Value == productId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _productRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("New Name", result.Value.Name);
        Assert.Equal("New Description", result.Value.Description);
        Assert.Equal(15.00m, result.Value.Price);
        Assert.Equal("New Category", result.Value.Category);
        Assert.Equal("New Brand", result.Value.Brand);

        _productRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentProduct_ReturnsFailure()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new UpdateProductCommand(productId, "New Name", "Description", 10.00m, "Category");

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<ProductId>(p => p.Value == productId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.IsType<NotFoundError>(result.Error);
        Assert.Equal("Product.NotFound", result.Error.Code);

        _productRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNewImages_UpdatesImages()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = Product.Create("SKU-001", "Product", "Description", 10.00m, "Category");
        product.AddImage("https://example.com/old-image.jpg");

        var command = new UpdateProductCommand(
            productId,
            "Product",
            "Description",
            10.00m,
            "Category",
            Images: new List<string> { "https://example.com/new-image1.jpg", "https://example.com/new-image2.jpg" });

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<ProductId>(p => p.Value == productId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _productRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Images.Count);
        Assert.Contains("https://example.com/new-image1.jpg", result.Value.Images);
        Assert.Contains("https://example.com/new-image2.jpg", result.Value.Images);
    }

    [Fact]
    public async Task Handle_WithNewTags_UpdatesTags()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = Product.Create("SKU-001", "Product", "Description", 10.00m, "Category");
        product.AddTag("old-tag");

        var command = new UpdateProductCommand(
            productId,
            "Product",
            "Description",
            10.00m,
            "Category",
            Tags: new List<string> { "new-tag1", "new-tag2" });

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<ProductId>(p => p.Value == productId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _productRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Tags.Count);
    }
}
