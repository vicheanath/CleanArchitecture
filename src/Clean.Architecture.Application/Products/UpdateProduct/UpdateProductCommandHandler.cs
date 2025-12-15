using Clean.Architecture.Application.Common.Interfaces;
using Clean.Architecture.Application.Products.DTOs;
using Clean.Architecture.Domain.Products;
using Shared.Errors;
using Shared.Messaging;
using Shared.Results;

namespace Clean.Architecture.Application.Products.UpdateProduct;

/// <summary>
/// Handler for updating an existing product.
/// </summary>
public class UpdateProductCommandHandler : ICommandHandler<UpdateProductCommand, ProductDto>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProductDto>> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        // Get the existing product
        var product = await _productRepository.GetByIdAsync(ProductId.Create(command.Id), cancellationToken);
        if (product == null)
        {
            return Result.Failure<ProductDto>(
                new NotFoundError("Product.NotFound", $"Product with ID '{command.Id}' was not found"));
        }

        // Update price if changed
        if (product.ProductPricing.RegularPrice != command.Price)
        {
            product.UpdatePrice(command.Price);
        }

        // Update sale information if provided
        if (command.SalePrice.HasValue || product.ProductPricing.SalePrice != command.SalePrice)
        {
            product.UpdateSaleInfo(command.SalePrice, command.SaleStartDate, command.SaleEndDate);
        }

        // Update product information
        product.UpdateInfo(
            command.Name,
            command.Description,
            command.Category,
            command.Brand,
            command.Weight,
            command.Dimensions,
            command.Color,
            command.Size,
            command.MetaTitle,
            command.MetaDescription,
            command.RequiresShipping,
            command.ShippingWeight,
            command.IsFeatured,
            command.SortOrder);

        // Update images
        if (command.Images != null)
        {
            // Remove all existing images
            var existingImages = product.Images.ImageUrls.ToList();
            foreach (var imageUrl in existingImages)
            {
                product.RemoveImage(imageUrl);
            }

            // Add new images
            foreach (var imageUrl in command.Images)
            {
                if (!string.IsNullOrWhiteSpace(imageUrl))
                {
                    product.AddImage(imageUrl);
                }
            }
        }

        // Update tags
        if (command.Tags != null)
        {
            // Remove all existing tags
            var existingTags = product.Tags.Tags.ToList();
            foreach (var tag in existingTags)
            {
                product.RemoveTag(tag);
            }

            // Add new tags
            foreach (var tag in command.Tags)
            {
                if (!string.IsNullOrWhiteSpace(tag))
                {
                    product.AddTag(tag);
                }
            }
        }

        await _productRepository.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Map to DTO
        var dto = new ProductDto(
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
            product.EffectivePrice,
            product.IsOnSale,
            product.IsActive,
            product.CreatedOnUtc);

        return Result.Success(dto);
    }
}
