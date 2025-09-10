using Clean.Architecture.Application.Products.DTOs;
using Clean.Architecture.Domain.Products;
using Shared.Errors;
using Shared.Messaging;
using Shared.Results;

namespace Clean.Architecture.Application.Products.CreateProduct;

/// <summary>
/// Handler for creating a new product with automatic inventory setup.
/// </summary>
public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, CreateProductResult>
{
    private readonly IProductRepository _productRepository;

    public CreateProductCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<CreateProductResult>> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        // Check if product with same SKU already exists
        var existingProduct = await _productRepository.GetBySkuAsync(command.Sku, cancellationToken);
        if (existingProduct != null)
        {
            return Result.Failure<CreateProductResult>(
                new ConflictError("Product.DuplicateSku", $"Product with SKU '{command.Sku}' already exists"));
        }

        // Create the product (this will raise ProductCreated event which creates inventory)
        var product = Product.Create(
            command.Sku,
            command.Name,
            command.Description,
            command.Price,
            command.Category);

        await _productRepository.AddAsync(product, cancellationToken);

        var result = new CreateProductResult(
            product.Id.Value,
            product.Sku,
            product.Name,
            product.Description,
            product.Price,
            product.Category,
            product.CreatedOnUtc);

        return Result.Success(result);
    }
}
