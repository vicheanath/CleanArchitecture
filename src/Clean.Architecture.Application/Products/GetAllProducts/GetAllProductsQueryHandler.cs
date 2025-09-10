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

        var productDtos = products.Select(p => new ProductDto(
            p.Id.Value,
            p.Sku,
            p.Name,
            p.Description,
            p.Price,
            p.Category,
            p.IsActive,
            p.CreatedOnUtc)).ToList();

        return Result.Success<IReadOnlyList<ProductDto>>(productDtos);
    }
}
