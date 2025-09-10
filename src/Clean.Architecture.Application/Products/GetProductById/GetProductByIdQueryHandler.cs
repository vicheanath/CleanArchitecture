using Clean.Architecture.Application.Products.DTOs;
using Clean.Architecture.Domain.Products;
using Shared.Messaging;
using Shared.Results;

namespace Clean.Architecture.Application.Products.GetProductById;

/// <summary>
/// Handler for getting a product by its ID
/// </summary>
public class GetProductByIdQueryHandler : IQueryHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly IProductRepository _productRepository;

    public GetProductByIdQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<ProductDto?>> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(ProductId.Create(query.Id), cancellationToken);

        if (product == null)
        {
            return Result.Failure<ProductDto?>(ProductErrors.NotFoundWithId(query.Id));
        }

        var productDto = new ProductDto(
            product.Id.Value,
            product.Sku,
            product.Name,
            product.Description,
            product.Price,
            product.Category,
            product.IsActive,
            product.CreatedOnUtc);

        return Result.Success<ProductDto?>(productDto);
    }
}
