using Clean.Architecture.Application.Common.Interfaces;
using Clean.Architecture.Application.Products;
using Clean.Architecture.Application.Products.DeleteProduct;
using Clean.Architecture.Domain.Products;
using Moq;
using Shared.Errors;
using Shared.Results;

namespace Clean.Architecture.Application.UnitTests.Products;

public class DeleteProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeleteProductCommandHandler _handler;

    public DeleteProductCommandHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new DeleteProductCommandHandler(_productRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingProduct_DeletesProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = Product.Create("SKU-001", "Product", "Description", 10.00m, "Category");

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<ProductId>(p => p.Value == productId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _productRepositoryMock
            .Setup(x => x.DeleteAsync(It.Is<ProductId>(p => p.Value == productId), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new DeleteProductCommand(productId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        _productRepositoryMock.Verify(x => x.DeleteAsync(It.Is<ProductId>(p => p.Value == productId), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentProduct_ReturnsFailure()
    {
        // Arrange
        var productId = Guid.NewGuid();

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<ProductId>(p => p.Value == productId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var command = new DeleteProductCommand(productId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.IsType<NotFoundError>(result.Error);
        Assert.Equal("Product.NotFound", result.Error.Code);

        _productRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<ProductId>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
