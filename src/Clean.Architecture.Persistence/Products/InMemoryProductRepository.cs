using Clean.Architecture.Application.Products;
using Clean.Architecture.Domain.Products;
using Clean.Architecture.Persistence.Common;
using System.Collections.Concurrent;

namespace Clean.Architecture.Persistence.Products;

/// <summary>
/// Binary file-based implementation of the Product repository for development/testing purposes
/// </summary>
public class InMemoryProductRepository : BinaryFileRepository<Guid, Product>, IProductRepository
{
    public InMemoryProductRepository() : base("products.dat")
    {
    }

    protected override Guid GetEntityKey(Product entity)
    {
        return entity.Id.Value;
    }

    protected override bool TryParseKey(string keyString, out Guid key)
    {
        return Guid.TryParse(keyString, out key);
    }

    public Task<Product?> GetByIdAsync(ProductId id, CancellationToken cancellationToken = default)
    {
        var product = GetEntity(id.Value);
        return Task.FromResult(product);
    }

    public Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        var product = GetAllEntities().FirstOrDefault(p => p.Sku == sku);
        return Task.FromResult(product);
    }

    public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var products = GetAllEntities().ToList();
        return Task.FromResult<IReadOnlyList<Product>>(products);
    }

    public Task<IReadOnlyList<Product>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        var products = GetAllEntities().Where(p => p.Category == category).ToList();
        return Task.FromResult<IReadOnlyList<Product>>(products);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await AddEntityAsync(product);
    }

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        await UpdateEntityAsync(product);
    }

    public async Task DeleteAsync(ProductId id, CancellationToken cancellationToken = default)
    {
        await RemoveEntityAsync(id.Value);
    }
}
