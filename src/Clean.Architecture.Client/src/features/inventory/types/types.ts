// Inventory types
export interface InventoryItemResponse {
  id: string;
  productSku: string;
  quantity: number;
  reservedQuantity: number;
  availableQuantity: number;
  minimumStockLevel: number;
  isOutOfStock: boolean;
  isBelowMinimumStock: boolean;
  createdOnUtc: string;
  modifiedOnUtc: string | null;
}

export interface LowStockItemResponse {
  id: string;
  productSku: string;
  quantity: number;
  minimumStockLevel: number;
  stockDeficit: number;
}

export interface CreateInventoryItemRequest {
  productSku: string;
  initialQuantity: number;
  minimumStockLevel: number;
}

export interface AdjustInventoryStockRequest {
  quantityChange: number;
  reason?: string;
}

export interface ReserveInventoryRequest {
  quantity: number;
  reservationId: string;
  expiresAt?: string;
}
