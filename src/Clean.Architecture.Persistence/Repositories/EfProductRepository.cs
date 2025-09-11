using Clean.Architecture.Application.Products;
using Clean.Architecture.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Clean.Architecture.Persistence.Repositories;

public sealed class EfProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public EfProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(ProductId id, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.Sku == sku, cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var products = await _context.Products
            .Where(p => p.IsActive)
            .ToListAsync(cancellationToken);
        return products.AsReadOnly();
    }

    public async Task<IReadOnlyList<Product>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        var products = await _context.Products
            .Where(p => p.Category == category && p.IsActive)
            .ToListAsync(cancellationToken);
        return products.AsReadOnly();
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _context.Products.AddAsync(product, cancellationToken);
    }

    public Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        _context.Products.Update(product);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ProductId id, CancellationToken cancellationToken = default)
    {
        var product = _context.Products.Local.FirstOrDefault(p => p.Id == id) ??
                     _context.Products.FirstOrDefault(p => p.Id == id);
        if (product != null)
        {
            _context.Products.Remove(product);
        }
        return Task.CompletedTask;
    }
}