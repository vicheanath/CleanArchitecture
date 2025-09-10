namespace Clean.Architecture.Application.Products.DTOs;

/// <summary>
/// Data Transfer Objects for Product operations
/// </summary>
public record CreateProductResult(
    Guid Id,
    string Sku,
    string Name,
    string Description,
    decimal Price,
    string Category,
    DateTime CreatedOnUtc);

public record ProductDto(
    Guid Id,
    string Sku,
    string Name,
    string Description,
    decimal Price,
    string Category,
    bool IsActive,
    DateTime CreatedOnUtc);
