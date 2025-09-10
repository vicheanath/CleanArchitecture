using Clean.Architecture.Application.Orders;
using Clean.Architecture.Domain.Orders;
using Clean.Architecture.Persistence.Common;
using System.Collections.Concurrent;

namespace Clean.Architecture.Persistence.Orders;

/// <summary>
/// Binary file-based implementation of the Order repository for development/testing purposes
/// </summary>
public class InMemoryOrderRepository : BinaryFileRepository<Guid, Order>, IOrderRepository
{
    public InMemoryOrderRepository() : base("orders.dat")
    {
    }

    protected override Guid GetEntityKey(Order entity)
    {
        return entity.Id.Value;
    }

    protected override bool TryParseKey(string keyString, out Guid key)
    {
        return Guid.TryParse(keyString, out key);
    }

    public Task<Order?> GetByIdAsync(OrderId id, CancellationToken cancellationToken = default)
    {
        var order = GetEntity(id.Value);
        return Task.FromResult(order);
    }

    public Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var orders = GetAllEntities().ToList();
        return Task.FromResult<IReadOnlyList<Order>>(orders);
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        await AddEntityAsync(order);
    }

    public async Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        await UpdateEntityAsync(order);
    }

    public async Task DeleteAsync(OrderId id, CancellationToken cancellationToken = default)
    {
        await RemoveEntityAsync(id.Value);
    }
}
