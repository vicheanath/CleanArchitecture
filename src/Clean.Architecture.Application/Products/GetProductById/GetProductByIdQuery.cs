using Clean.Architecture.Application.Products.DTOs;
using Shared.Messaging;

namespace Clean.Architecture.Application.Products.GetProductById;

/// <summary>
/// Query to get a product by its ID
/// </summary>
public record GetProductByIdQuery(Guid Id) : IQuery<ProductDto?>;
