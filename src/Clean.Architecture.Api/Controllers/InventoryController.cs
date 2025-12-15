using Clean.Architecture.Application.Inventory.AdjustInventoryStock;
using Clean.Architecture.Application.Inventory.CreateInventoryItem;
using Clean.Architecture.Application.Inventory.ReserveInventory;
using Clean.Architecture.Application.Inventory.GetInventoryItem;
using Clean.Architecture.Application.Inventory.GetLowStockItems;
using Microsoft.AspNetCore.Mvc;
using Shared.Messaging;
using Shared.Results;
using InventoryItemResponse = Clean.Architecture.Application.Inventory.GetInventoryItem.InventoryItemResponse;
using LowStockItemResponse = Clean.Architecture.Application.Inventory.GetLowStockItems.LowStockItemResponse;

namespace Clean.Architecture.Api.Controllers;

/// <summary>
/// Inventory controller for managing inventory operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IQueryHandler<GetInventoryItemQuery, InventoryItemResponse> _getInventoryItemHandler;
    private readonly IQueryHandler<GetLowStockItemsQuery, IEnumerable<LowStockItemResponse>> _getLowStockItemsHandler;
    private readonly ICommandHandler<CreateInventoryItemCommand, Guid> _createInventoryItemHandler;
    private readonly ICommandHandler<AdjustInventoryStockCommand> _adjustInventoryStockHandler;
    private readonly ICommandHandler<ReserveInventoryCommand> _reserveInventoryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="InventoryController"/> class.
    /// </summary>
    public InventoryController(
        IQueryHandler<GetInventoryItemQuery, InventoryItemResponse> getInventoryItemHandler,
        IQueryHandler<GetLowStockItemsQuery, IEnumerable<LowStockItemResponse>> getLowStockItemsHandler,
        ICommandHandler<CreateInventoryItemCommand, Guid> createInventoryItemHandler,
        ICommandHandler<AdjustInventoryStockCommand> adjustInventoryStockHandler,
        ICommandHandler<ReserveInventoryCommand> reserveInventoryHandler)
    {
        _getInventoryItemHandler = getInventoryItemHandler;
        _getLowStockItemsHandler = getLowStockItemsHandler;
        _createInventoryItemHandler = createInventoryItemHandler;
        _adjustInventoryStockHandler = adjustInventoryStockHandler;
        _reserveInventoryHandler = reserveInventoryHandler;
    }

    /// <summary>
    /// Gets an inventory item by its identifier.
    /// </summary>
    /// <param name="id">The inventory item identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The inventory item.</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Result<InventoryItemResponse>>> GetInventoryItem(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetInventoryItemQuery(id);
        var result = await _getInventoryItemHandler.Handle(query, cancellationToken);
        return result;
    }

    /// <summary>
    /// Gets all inventory items that are below their minimum stock level.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The collection of low stock items.</returns>
    [HttpGet("low-stock")]
    public async Task<ActionResult<Result<IEnumerable<LowStockItemResponse>>>> GetLowStockItems(
        CancellationToken cancellationToken)
    {
        var query = new GetLowStockItemsQuery();
        var result = await _getLowStockItemsHandler.Handle(query, cancellationToken);
        return result;
    }

    /// <summary>
    /// Creates a new inventory item.
    /// </summary>
    /// <param name="request">The create inventory item request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created inventory item identifier.</returns>
    [HttpPost]
    public async Task<ActionResult<Result<Guid>>> CreateInventoryItem(
        [FromBody] CreateInventoryItemRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateInventoryItemCommand(
            request.ProductSku,
            request.InitialQuantity,
            request.MinimumStockLevel);

        var result = await _createInventoryItemHandler.Handle(command, cancellationToken);

        if (result.IsSuccess)
        {
            return CreatedAtAction(
                nameof(GetInventoryItem),
                new { id = result.Value },
                result);
        }

        return result;
    }

    /// <summary>
    /// Adjusts the inventory stock for an item.
    /// </summary>
    /// <param name="id">The inventory item identifier.</param>
    /// <param name="request">The adjust stock request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    [HttpPatch("{id:guid}/adjust-stock")]
    public async Task<ActionResult<Result>> AdjustInventoryStock(
        Guid id,
        [FromBody] AdjustInventoryStockRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AdjustInventoryStockCommand(id, request.QuantityChange, request.Reason);
        var result = await _adjustInventoryStockHandler.Handle(command, cancellationToken);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return result;
    }

    /// <summary>
    /// Reserves inventory for an order or other operation.
    /// </summary>
    /// <param name="id">The inventory item identifier.</param>
    /// <param name="request">The reserve inventory request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    [HttpPost("{id:guid}/reservations")]
    public async Task<ActionResult<Result>> ReserveInventory(
        Guid id,
        [FromBody] ReserveInventoryRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ReserveInventoryCommand(id, request.Quantity, request.ReservationId, request.ExpiresAt);
        var result = await _reserveInventoryHandler.Handle(command, cancellationToken);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return result;
    }
}

/// <summary>
/// Request for creating an inventory item.
/// </summary>
/// <param name="ProductSku">The product SKU.</param>
/// <param name="InitialQuantity">The initial quantity.</param>
/// <param name="MinimumStockLevel">The minimum stock level.</param>
public record CreateInventoryItemRequest(
    string ProductSku,
    int InitialQuantity,
    int MinimumStockLevel);

/// <summary>
/// Request for adjusting inventory stock.
/// </summary>
/// <param name="QuantityChange">The quantity change (positive for increase, negative for decrease).</param>
/// <param name="Reason">The reason for the adjustment.</param>
public record AdjustInventoryStockRequest(
    int QuantityChange,
    string? Reason);

/// <summary>
/// Request for reserving inventory.
/// </summary>
/// <param name="Quantity">The quantity to reserve.</param>
/// <param name="ReservationId">The reservation identifier.</param>
/// <param name="ExpiresAt">The expiration date and time for the reservation.</param>
public record ReserveInventoryRequest(
    int Quantity,
    string ReservationId,
    DateTime? ExpiresAt);
