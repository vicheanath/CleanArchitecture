using Clean.Architecture.Application.Common.Interfaces;
using Clean.Architecture.Domain.Products;
using Shared.Errors;
using Shared.Messaging;
using Shared.Results;

namespace Clean.Architecture.Application.Products.DeleteProduct;

/// <summary>
/// Handler for deleting a product.
/// </summary>
public class DeleteProductCommandHandler : ICommandHandler<DeleteProductCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        var productId = new ProductId(command.Id);

        // Check if product exists
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
        if (product == null)
        {
            return Result.Failure(
                new NotFoundError("Product.NotFound", $"Product with ID '{command.Id}' was not found"));
        }

        // Delete the product
        await _productRepository.DeleteAsync(productId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
