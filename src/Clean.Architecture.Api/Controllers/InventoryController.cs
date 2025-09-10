using Clean.Architecture.Application.Inventory.Commands.AdjustInventoryStock;
using Clean.Architecture.Application.Inventory.Commands.CreateInventoryItem;
using Clean.Architecture.Application.Inventory.Commands.ReserveInventory;
using Clean.Architecture.Application.Inventory.Queries.GetInventoryItem;
using Clean.Architecture.Application.Inventory.Queries.GetLowStockItems;
using Microsoft.AspNetCore.Mvc;
using Shared.Exceptions;
using Shared.Messaging;

namespace Clean.Architecture.Api.Controllers;

/// <summary>
/// Inventory controller for managing inventory operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    /// <summary>
    /// Initializes a new instance of the <see cref="InventoryController"/> class.
    /// </summary>
    /// <param name="dispatcher">The dispatcher.</param>
    public InventoryController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    /// <summary>
    /// Gets an inventory item by its identifier.
    /// </summary>
    /// <param name="id">The inventory item identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The inventory item.</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InventoryItemResponse>> GetInventoryItem(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetInventoryItemQuery(id);
            var result = await _dispatcher.QueryAsync<GetInventoryItemQuery, InventoryItemResponse>(query, cancellationToken);
            return Ok(result);
        }
        catch (DomainException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Gets all inventory items that are below their minimum stock level.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The collection of low stock items.</returns>
    [HttpGet("low-stock")]
    public async Task<ActionResult<IEnumerable<LowStockItemResponse>>> GetLowStockItems(
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetLowStockItemsQuery();
            var result = await _dispatcher.QueryAsync<GetLowStockItemsQuery, IEnumerable<LowStockItemResponse>>(query, cancellationToken);
            return Ok(result);
        }
        catch (DomainException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Creates a new inventory item.
    /// </summary>
    /// <param name="request">The create inventory item request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created inventory item identifier.</returns>
    [HttpPost]
    public async Task<ActionResult<Guid>> CreateInventoryItem(
        [FromBody] CreateInventoryItemRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateInventoryItemCommand(
                request.ProductSku,
                request.InitialQuantity,
                request.MinimumStockLevel);

            var result = await _dispatcher.CommandAsync<CreateInventoryItemCommand, Guid>(command, cancellationToken);

            return CreatedAtAction(
                nameof(GetInventoryItem),
                new { id = result },
                result);
        }
        catch (DomainException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Adjusts the inventory stock for an item.
    /// </summary>
    /// <param name="id">The inventory item identifier.</param>
    /// <param name="request">The adjust stock request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    [HttpPatch("{id:guid}/adjust-stock")]
    public async Task<ActionResult> AdjustInventoryStock(
        Guid id,
        [FromBody] AdjustInventoryStockRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new AdjustInventoryStockCommand(id, request.QuantityChange, request.Reason);
            await _dispatcher.CommandAsync<AdjustInventoryStockCommand>(command, cancellationToken);
            return NoContent();
        }
        catch (DomainException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Reserves inventory for an order or other operation.
    /// </summary>
    /// <param name="id">The inventory item identifier.</param>
    /// <param name="request">The reserve inventory request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    [HttpPost("{id:guid}/reservations")]
    public async Task<ActionResult> ReserveInventory(
        Guid id,
        [FromBody] ReserveInventoryRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new ReserveInventoryCommand(id, request.Quantity, request.ReservationId, request.ExpiresAt);
            await _dispatcher.CommandAsync<ReserveInventoryCommand>(command, cancellationToken);
            return NoContent();
        }
        catch (DomainException ex)
        {
            return BadRequest(ex.Message);
        }
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
