using Clean.Architecture.Application.Common.Interfaces;
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
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
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
            command.Category,
            command.Brand,
            command.Weight,
            command.Dimensions,
            command.Color,
            command.Size,
            command.SalePrice,
            command.SaleStartDate,
            command.SaleEndDate,
            command.MetaTitle,
            command.MetaDescription,
            command.RequiresShipping,
            command.ShippingWeight,
            command.IsFeatured,
            command.SortOrder,
            command.Images,
            command.Tags);

        await _productRepository.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var result = new CreateProductResult(
            product.Id.Value,
            product.Sku,
            product.Name,
            product.Description,
            product.ProductPricing.RegularPrice,
            product.Category,
            product.Brand,
            product.ProductPhysicalAttributes.Weight ?? 0,
            product.ProductPhysicalAttributes.Dimensions ?? string.Empty,
            product.ProductPhysicalAttributes.Color ?? string.Empty,
            product.ProductPhysicalAttributes.Size ?? string.Empty,
            product.ProductPricing.SalePrice,
            product.ProductPricing.SaleStartDate,
            product.ProductPricing.SaleEndDate,
            product.ProductSeoMetadata.MetaTitle ?? string.Empty,
            product.ProductSeoMetadata.MetaDescription ?? string.Empty,
            product.ProductShippingInfo.RequiresShipping,
            product.ProductShippingInfo.ShippingWeight ?? 0,
            product.IsFeatured,
            product.SortOrder,
            product.Images.ImageUrls.ToList(),
            product.Tags.Tags.ToList(),
            product.CreatedOnUtc);

        return Result.Success(result);
    }
}
