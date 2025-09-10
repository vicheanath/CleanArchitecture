using Clean.Architecture.Application.Products.DTOs;
using Shared.Messaging;

namespace Clean.Architecture.Application.Products.GetAllProducts;

/// <summary>
/// Query to get all products
/// </summary>
public record GetAllProductsQuery() : IQuery<IReadOnlyList<ProductDto>>;
