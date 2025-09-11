using Clean.Architecture.Application.Products.DTOs;
using Shared.Messaging;
using Shared.Results;

namespace Clean.Architecture.Application.Products.GetAllProducts;

/// <summary>
/// Handler for getting all products
/// </summary>
public class GetAllProductsQueryHandler : IQueryHandler<GetAllProductsQuery, IReadOnlyList<ProductDto>>
{
    private readonly IProductRepository _productRepository;

    public GetAllProductsQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<IReadOnlyList<ProductDto>>> Handle(GetAllProductsQuery query, CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetAllAsync(cancellationToken);

        var productDtos = products.Select(product => new ProductDto(
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
            product.CreatedOnUtc)).ToList();

        return Result.Success<IReadOnlyList<ProductDto>>(productDtos);
    }
}
