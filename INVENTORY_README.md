# Inventory Management System

This document describes the inventory management system implemented with domain-driven design patterns and domain events.

## Overview

The inventory management system provides comprehensive inventory tracking with the following features:

- **Inventory Item Management**: Create and manage inventory items with product SKUs
- **Stock Adjustments**: Increase and decrease inventory quantities with audit trails
- **Inventory Reservations**: Reserve inventory for orders with expiration support
- **Low Stock Monitoring**: Automatic detection and alerting for low stock situations
- **Out of Stock Detection**: Automatic detection when items are completely out of stock
- **Domain Events**: Rich event system for business process automation

## Domain Model

### InventoryItem Entity

The `InventoryItem` is the main aggregate root that encapsulates all inventory-related business logic:

- **Id**: Unique identifier for the inventory item
- **ProductSku**: Product stock keeping unit (unique per item)
- **Quantity**: Current quantity in stock
- **MinimumStockLevel**: Threshold for low stock warnings
- **ReservedQuantity**: Total quantity reserved for orders
- **AvailableQuantity**: Quantity available for new reservations (Quantity - ReservedQuantity)

### Value Objects

#### InventoryReservation

Represents a reservation of inventory for a specific period:

- **ReservationId**: Unique identifier for the reservation
- **Quantity**: Amount reserved
- **ReservedAt**: Timestamp when reservation was made
- **ExpiresAt**: Optional expiration timestamp

## Domain Events

The inventory system publishes various domain events to enable loose coupling and business process automation:

### 1. InventoryItemCreatedDomainEvent

Raised when a new inventory item is created.

**Use cases:**

- Initialize inventory tracking in external systems
- Set up automated reorder rules
- Create audit log entries

### 2. InventoryStockIncreasedDomainEvent

Raised when inventory quantity is increased.

**Use cases:**

- Update inventory reports
- Notify sales teams of increased availability
- Update product availability on websites

### 3. InventoryStockDecreasedDomainEvent

Raised when inventory quantity is decreased.

**Use cases:**

- Update inventory reports
- Check if reorder is needed
- Update product availability displays

### 4. LowStockWarningDomainEvent

Raised when inventory falls to or below the minimum stock level.

**Use cases:**

- Send notifications to procurement teams
- Automatically create purchase orders
- Alert suppliers about potential stockouts
- Update dashboard warnings

### 5. OutOfStockDomainEvent

Raised when inventory quantity reaches zero.

**Use cases:**

- Send urgent notifications to management
- Disable product sales on websites
- Create high-priority purchase orders
- Notify customer service teams

### 6. InventoryReservedDomainEvent

Raised when inventory is reserved for an order.

**Use cases:**

- Update order management systems
- Notify fulfillment centers
- Create allocation workflows

### 7. InventoryReservationReleasedDomainEvent

Raised when a reservation is released (either expired or manually released).

**Use cases:**

- Make inventory available for new orders
- Update inventory availability displays
- Create audit trail entries

### 8. MinimumStockLevelUpdatedDomainEvent

Raised when the minimum stock level threshold is changed.

**Use cases:**

- Update reorder policies
- Recalculate low stock warnings
- Notify procurement teams of policy changes

## API Endpoints

### GET /api/inventory/{id}

Retrieves detailed information about a specific inventory item.

### GET /api/inventory/low-stock

Returns all inventory items that are currently below their minimum stock level.

### POST /api/inventory

Creates a new inventory item.

**Request Body:**

```json
{
  "productSku": "PROD-001",
  "initialQuantity": 100,
  "minimumStockLevel": 10
}
```

### PATCH /api/inventory/{id}/adjust-stock

Adjusts the stock quantity for an inventory item.

**Request Body:**

```json
{
  "quantityChange": -5,
  "reason": "Sale to customer"
}
```

### POST /api/inventory/{id}/reservations

Reserves inventory for an order or other operation.

**Request Body:**

```json
{
  "quantity": 2,
  "reservationId": "ORDER-123",
  "expiresAt": "2024-12-31T23:59:59Z"
}
```

## Event Handlers

The system includes several domain event handlers that demonstrate how to respond to inventory events:

### InventoryItemCreatedDomainEventHandler

Logs the creation of new inventory items and can trigger additional setup processes.

### LowStockWarningDomainEventHandler

Handles low stock situations by logging warnings and can be extended to send notifications or create purchase orders.

### OutOfStockDomainEventHandler

Handles out-of-stock situations with critical logging and can trigger emergency restocking procedures.

### InventoryStockChangedDomainEventHandler

Tracks all stock changes for audit purposes and business intelligence.

### InventoryReservationDomainEventHandler

Manages reservation lifecycle events and coordinates with order management systems.

## Business Rules

### Stock Adjustments

- Quantity changes must be non-zero
- Cannot decrease stock below zero
- All adjustments are tracked with optional reasons

### Reservations

- Cannot reserve more than available quantity
- Reservations can have expiration dates
- Expired reservations are automatically released
- Reservation IDs must be unique per item

### Stock Levels

- Minimum stock level must be zero or positive
- Low stock warnings trigger when quantity ≤ minimum stock level
- Out of stock events trigger when quantity = 0

## Usage Examples

### Creating an Inventory Item

```csharp
var inventoryItem = InventoryItem.Create("WIDGET-001", 50, 5);
// This raises: InventoryItemCreatedDomainEvent
```

### Adjusting Stock

```csharp
inventoryItem.IncreaseStock(20, "Restocking from supplier");
// This raises: InventoryStockIncreasedDomainEvent

inventoryItem.DecreaseStock(3, "Customer purchase");
// This raises: InventoryStockDecreasedDomainEvent
// May also raise: LowStockWarningDomainEvent or OutOfStockDomainEvent
```

### Managing Reservations

```csharp
inventoryItem.ReserveStock(5, "ORDER-789", DateTime.UtcNow.AddHours(24));
// This raises: InventoryReservedDomainEvent

inventoryItem.ReleaseReservation("ORDER-789");
// This raises: InventoryReservationReleasedDomainEvent
```

This inventory system provides a robust foundation for e-commerce, warehouse management, and other applications requiring sophisticated inventory tracking with automated business processes.
