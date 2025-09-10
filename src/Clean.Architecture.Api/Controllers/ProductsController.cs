using Clean.Architecture.Application.Products;
using Clean.Architecture.Application.Products.DTOs;
using Clean.Architecture.Application.Products.CreateProduct;
using Clean.Architecture.Application.Products.GetAllProducts;
using Clean.Architecture.Application.Products.GetProductById;
using Shared.Messaging;
using Microsoft.AspNetCore.Mvc;

namespace Clean.Architecture.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public ProductsController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetAllProducts(CancellationToken cancellationToken)
    {
        var products = await _dispatcher.QueryAsync<GetAllProductsQuery, IReadOnlyList<ProductDto>>(new GetAllProductsQuery(), cancellationToken);
        return Ok(products);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetProductById(Guid id, CancellationToken cancellationToken)
    {
        var product = await _dispatcher.QueryAsync<GetProductByIdQuery, ProductDto?>(new GetProductByIdQuery(id), cancellationToken);
        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<CreateProductResult>> CreateProduct(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateProductCommand(request.Sku, request.Name, request.Description, request.Price, request.Category);
        var result = await _dispatcher.CommandAsync<CreateProductCommand, CreateProductResult>(command, cancellationToken);

        return CreatedAtAction(
            nameof(GetProductById),
            new { id = result.Id },
            result);
    }
}

public record CreateProductRequest(string Sku, string Name, string Description, decimal Price, string Category);
