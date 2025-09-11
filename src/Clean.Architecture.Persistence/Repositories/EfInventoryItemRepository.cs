using Clean.Architecture.Domain.Inventory;
using Microsoft.EntityFrameworkCore;

namespace Clean.Architecture.Persistence.Repositories;

public sealed class EfInventoryItemRepository : IInventoryItemRepository
{
    private readonly ApplicationDbContext _context;

    public EfInventoryItemRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<InventoryItem?> GetByIdAsync(InventoryItemId id, CancellationToken cancellationToken = default)
    {
        return await _context.InventoryItems
            .Include(i => i.Reservations)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<InventoryItem?> GetByProductSkuAsync(string productSku, CancellationToken cancellationToken = default)
    {
        return await _context.InventoryItems
            .Include(i => i.Reservations)
            .FirstOrDefaultAsync(i => i.ProductSku == productSku, cancellationToken);
    }

    public async Task<IEnumerable<InventoryItem>> GetItemsBelowMinimumStockAsync(CancellationToken cancellationToken = default)
    {
        return await _context.InventoryItems
            .Include(i => i.Reservations)
            .Where(i => i.Quantity <= i.MinimumStockLevel)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<InventoryItem>> GetOutOfStockItemsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.InventoryItems
            .Include(i => i.Reservations)
            .Where(i => i.Quantity <= 0)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<InventoryItem>> GetItemsWithExpiredReservationsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.InventoryItems
            .Include(i => i.Reservations)
            .Where(i => i.Reservations.Any(r => r.ExpiresAt.HasValue && r.ExpiresAt.Value < now))
            .ToListAsync(cancellationToken);
    }

    public void Add(InventoryItem inventoryItem)
    {
        _context.InventoryItems.Add(inventoryItem);
    }

    public async Task AddAsync(InventoryItem inventoryItem, CancellationToken cancellationToken = default)
    {
        await _context.InventoryItems.AddAsync(inventoryItem, cancellationToken);
    }

    public void Update(InventoryItem inventoryItem)
    {
        _context.InventoryItems.Update(inventoryItem);
    }

    public Task UpdateAsync(InventoryItem inventoryItem, CancellationToken cancellationToken = default)
    {
        _context.InventoryItems.Update(inventoryItem);
        return Task.CompletedTask;
    }

    public void Remove(InventoryItem inventoryItem)
    {
        _context.InventoryItems.Remove(inventoryItem);
    }

    public Task RemoveAsync(InventoryItem inventoryItem, CancellationToken cancellationToken = default)
    {
        _context.InventoryItems.Remove(inventoryItem);
        return Task.CompletedTask;
    }
}
