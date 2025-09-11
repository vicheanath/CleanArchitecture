using Clean.Architecture.Application.Products;
using Clean.Architecture.Application.Products.DTOs;
using Clean.Architecture.Application.Products.CreateProduct;
using Clean.Architecture.Application.Products.DeleteProduct;
using Clean.Architecture.Application.Products.GetAllProducts;
using Clean.Architecture.Application.Products.GetProductById;
using Shared.Messaging;
using Microsoft.AspNetCore.Mvc;

namespace Clean.Architecture.Api.Controllers;

/// <summary>
/// Controller for managing products.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductsController"/> class.
    /// </summary>
    /// <param name="dispatcher">The command and query dispatcher.</param>
    public ProductsController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    /// <summary>
    /// Gets all products.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of products.</returns>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetAllProducts(CancellationToken cancellationToken)
    {
        var products = await _dispatcher.QueryAsync<GetAllProductsQuery, IReadOnlyList<ProductDto>>(new GetAllProductsQuery(), cancellationToken);
        return Ok(products);
    }

    /// <summary>
    /// Gets a product by its identifier.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The product.</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetProductById(Guid id, CancellationToken cancellationToken)
    {
        var product = await _dispatcher.QueryAsync<GetProductByIdQuery, ProductDto?>(new GetProductByIdQuery(id), cancellationToken);
        return Ok(product);
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="request">The create product request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created product.</returns>
    [HttpPost]
    public async Task<ActionResult<CreateProductResult>> CreateProduct(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateProductCommand(
            request.Sku,
            request.Name,
            request.Description,
            request.Price,
            request.Category,
            request.Brand,
            request.Weight,
            request.Dimensions,
            request.Color,
            request.Size,
            request.SalePrice,
            request.SaleStartDate,
            request.SaleEndDate,
            request.MetaTitle,
            request.MetaDescription,
            request.RequiresShipping,
            request.ShippingWeight,
            request.IsFeatured,
            request.SortOrder,
            request.Images,
            request.Tags);

        var result = await _dispatcher.CommandAsync<CreateProductCommand, CreateProductResult>(command, cancellationToken);

        return CreatedAtAction(
            nameof(GetProductById),
            new { id = result.Id },
            result);
    }

    /// <summary>
    /// Deletes a product.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteProduct(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteProductCommand(id);
        await _dispatcher.CommandAsync(command, cancellationToken);
        return NoContent();
    }
}

/// <summary>
/// Request model for creating a new product.
/// </summary>
/// <param name="Sku">The product SKU.</param>
/// <param name="Name">The product name.</param>
/// <param name="Description">The product description.</param>
/// <param name="Price">The product price.</param>
/// <param name="Category">The product category.</param>
/// <param name="Brand">The product brand.</param>
/// <param name="Weight">The product weight.</param>
/// <param name="Dimensions">The product dimensions.</param>
/// <param name="Color">The product color.</param>
/// <param name="Size">The product size.</param>
/// <param name="SalePrice">The sale price.</param>
/// <param name="SaleStartDate">The sale start date.</param>
/// <param name="SaleEndDate">The sale end date.</param>
/// <param name="MetaTitle">The meta title for SEO.</param>
/// <param name="MetaDescription">The meta description for SEO.</param>
/// <param name="RequiresShipping">Whether the product requires shipping.</param>
/// <param name="ShippingWeight">The shipping weight.</param>
/// <param name="IsFeatured">Whether the product is featured.</param>
/// <param name="SortOrder">The sort order.</param>
/// <param name="Images">The product images.</param>
/// <param name="Tags">The product tags.</param>
public record CreateProductRequest(
    string Sku,
    string Name,
    string Description,
    decimal Price,
    string Category,
    string Brand = "",
    decimal Weight = 0,
    string Dimensions = "",
    string Color = "",
    string Size = "",
    decimal? SalePrice = null,
    DateTime? SaleStartDate = null,
    DateTime? SaleEndDate = null,
    string MetaTitle = "",
    string MetaDescription = "",
    bool RequiresShipping = true,
    decimal ShippingWeight = 0,
    bool IsFeatured = false,
    int SortOrder = 0,
    List<string>? Images = null,
    List<string>? Tags = null);
