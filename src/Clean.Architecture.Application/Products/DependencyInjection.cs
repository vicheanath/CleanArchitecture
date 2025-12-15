using Clean.Architecture.Application.Products.CreateProduct;
using Clean.Architecture.Application.Products.DeleteProduct;
using Clean.Architecture.Application.Products.GetAllProducts;
using Clean.Architecture.Application.Products.GetProductById;
using Clean.Architecture.Application.Products.UpdateProduct;
using Clean.Architecture.Application.Products.DTOs;
using Clean.Architecture.Application.Products.EventHandlers;
using Clean.Architecture.Domain.Products;
using Microsoft.Extensions.DependencyInjection;
using Shared.Messaging;

namespace Clean.Architecture.Application.Products;

/// <summary>
/// Dependency injection extensions for Products feature.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all Products feature services.
    /// </summary>
    public static IServiceCollection AddProducts(this IServiceCollection services)
    {
        // Register Command Handlers
        services.AddScoped<ICommandHandler<CreateProductCommand, CreateProductResult>, CreateProductCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateProductCommand, ProductDto>, UpdateProductCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteProductCommand>, DeleteProductCommandHandler>();

        // Register Query Handlers
        services.AddScoped<IQueryHandler<GetAllProductsQuery, IReadOnlyList<ProductDto>>, GetAllProductsQueryHandler>();
        services.AddScoped<IQueryHandler<GetProductByIdQuery, ProductDto?>, GetProductByIdQueryHandler>();

        // Register Domain Event Handlers
        services.AddScoped<IDomainEventHandler<ProductCreatedDomainEvent>, ProductCreatedDomainEventHandler>();

        return services;
    }
}
