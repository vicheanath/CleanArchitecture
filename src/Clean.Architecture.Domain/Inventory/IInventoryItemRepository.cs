namespace Clean.Architecture.Domain.Inventory;

/// <summary>
/// Represents the inventory item repository interface.
/// </summary>
public interface IInventoryItemRepository
{
    /// <summary>
    /// Gets an inventory item by its identifier.
    /// </summary>
    /// <param name="id">The inventory item identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The inventory item if found; otherwise, null.</returns>
    Task<InventoryItem?> GetByIdAsync(InventoryItemId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an inventory item by its product SKU.
    /// </summary>
    /// <param name="productSku">The product SKU.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The inventory item if found; otherwise, null.</returns>
    Task<InventoryItem?> GetByProductSkuAsync(string productSku, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all inventory items that are below their minimum stock level.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The collection of inventory items below minimum stock level.</returns>
    Task<IEnumerable<InventoryItem>> GetItemsBelowMinimumStockAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all inventory items that are out of stock.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The collection of out of stock inventory items.</returns>
    Task<IEnumerable<InventoryItem>> GetOutOfStockItemsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all inventory items with expired reservations.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The collection of inventory items with expired reservations.</returns>
    Task<IEnumerable<InventoryItem>> GetItemsWithExpiredReservationsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an inventory item to the repository.
    /// </summary>
    /// <param name="inventoryItem">The inventory item to add.</param>
    void Add(InventoryItem inventoryItem);

    /// <summary>
    /// Adds an inventory item to the repository asynchronously.
    /// </summary>
    /// <param name="inventoryItem">The inventory item to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AddAsync(InventoryItem inventoryItem, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an inventory item in the repository.
    /// </summary>
    /// <param name="inventoryItem">The inventory item to update.</param>
    void Update(InventoryItem inventoryItem);

    /// <summary>
    /// Updates an inventory item in the repository asynchronously.
    /// </summary>
    /// <param name="inventoryItem">The inventory item to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task UpdateAsync(InventoryItem inventoryItem, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an inventory item from the repository.
    /// </summary>
    /// <param name="inventoryItem">The inventory item to remove.</param>
    void Remove(InventoryItem inventoryItem);

    /// <summary>
    /// Removes an inventory item from the repository asynchronously.
    /// </summary>
    /// <param name="inventoryItem">The inventory item to remove.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task RemoveAsync(InventoryItem inventoryItem, CancellationToken cancellationToken = default);
}
