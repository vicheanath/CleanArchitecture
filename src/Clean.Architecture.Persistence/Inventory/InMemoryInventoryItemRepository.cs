using Clean.Architecture.Domain.Inventory;
using Clean.Architecture.Persistence.Common;
using System.Collections.Concurrent;

namespace Clean.Architecture.Persistence.Inventory;

/// <summary>
/// Binary file-based implementation of the inventory item repository.
/// </summary>
internal sealed class InMemoryInventoryItemRepository : BinaryFileRepository<InventoryItemId, InventoryItem>, IInventoryItemRepository
{
    public InMemoryInventoryItemRepository() : base("inventory-items.dat")
    {
    }

    protected override InventoryItemId GetEntityKey(InventoryItem entity)
    {
        return entity.Id;
    }

    protected override bool TryParseKey(string keyString, out InventoryItemId key)
    {
        if (Guid.TryParse(keyString, out var guid))
        {
            key = new InventoryItemId(guid);
            return true;
        }
        key = default!;
        return false;
    }

    /// <inheritdoc />
    public Task<InventoryItem?> GetByIdAsync(InventoryItemId id, CancellationToken cancellationToken = default)
    {
        var inventoryItem = GetEntity(id);
        return Task.FromResult(inventoryItem);
    }

    /// <inheritdoc />
    public Task<InventoryItem?> GetByProductSkuAsync(string productSku, CancellationToken cancellationToken = default)
    {
        var inventoryItem = GetAllEntities().FirstOrDefault(item => item.ProductSku == productSku);
        return Task.FromResult(inventoryItem);
    }

    /// <inheritdoc />
    public Task<IEnumerable<InventoryItem>> GetItemsBelowMinimumStockAsync(CancellationToken cancellationToken = default)
    {
        var items = GetAllEntities().Where(item => item.IsBelowMinimumStock);
        return Task.FromResult(items);
    }

    /// <inheritdoc />
    public Task<IEnumerable<InventoryItem>> GetOutOfStockItemsAsync(CancellationToken cancellationToken = default)
    {
        var items = GetAllEntities().Where(item => item.IsOutOfStock);
        return Task.FromResult(items);
    }

    /// <inheritdoc />
    public Task<IEnumerable<InventoryItem>> GetItemsWithExpiredReservationsAsync(CancellationToken cancellationToken = default)
    {
        var items = GetAllEntities().Where(item =>
            item.Reservations.Any(r => r.IsExpired));
        return Task.FromResult(items);
    }

    /// <inheritdoc />
    public void Add(InventoryItem inventoryItem)
    {
        _entities.TryAdd(inventoryItem.Id, inventoryItem);
        _ = Task.Run(() => SaveToFileAsync());
    }

    /// <inheritdoc />
    public async Task AddAsync(InventoryItem inventoryItem, CancellationToken cancellationToken = default)
    {
        await AddEntityAsync(inventoryItem);
    }

    /// <inheritdoc />
    public void Update(InventoryItem inventoryItem)
    {
        _entities.AddOrUpdate(inventoryItem.Id, inventoryItem, (_, _) => inventoryItem);
        _ = Task.Run(() => SaveToFileAsync());
    }

    /// <inheritdoc />
    public async Task UpdateAsync(InventoryItem inventoryItem, CancellationToken cancellationToken = default)
    {
        await UpdateEntityAsync(inventoryItem);
    }

    /// <inheritdoc />
    public void Remove(InventoryItem inventoryItem)
    {
        _entities.TryRemove(inventoryItem.Id, out _);
        _ = Task.Run(() => SaveToFileAsync());
    }

    /// <inheritdoc />
    public async Task RemoveAsync(InventoryItem inventoryItem, CancellationToken cancellationToken = default)
    {
        await RemoveEntityAsync(inventoryItem.Id);
    }
}
