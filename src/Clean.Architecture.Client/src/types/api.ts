// Product types
export interface ProductDto {
  id: string;
  sku: string;
  name: string;
  description: string;
  price: number;
  category: string;
  brand: string;
  weight: number;
  dimensions: string;
  color: string;
  size: string;
  salePrice?: number;
  saleStartDate?: string;
  saleEndDate?: string;
  metaTitle: string;
  metaDescription: string;
  requiresShipping: boolean;
  shippingWeight: number;
  isFeatured: boolean;
  sortOrder: number;
  images: string[];
  tags: string[];
  effectivePrice: number;
  isOnSale: boolean;
  isActive: boolean;
  createdOnUtc: string;
}

export interface CreateProductRequest {
  sku: string;
  name: string;
  description: string;
  price: number;
  category: string;
  brand?: string;
  weight?: number;
  dimensions?: string;
  color?: string;
  size?: string;
  salePrice?: number;
  saleStartDate?: string;
  saleEndDate?: string;
  metaTitle?: string;
  metaDescription?: string;
  requiresShipping?: boolean;
  shippingWeight?: number;
  isFeatured?: boolean;
  sortOrder?: number;
  images?: string[];
  tags?: string[];
}

export interface CreateProductResult {
  id: string;
  sku: string;
  name: string;
  description: string;
  price: number;
  category: string;
  brand: string;
  weight: number;
  dimensions: string;
  color: string;
  size: string;
  salePrice?: number;
  saleStartDate?: string;
  saleEndDate?: string;
  metaTitle: string;
  metaDescription: string;
  requiresShipping: boolean;
  shippingWeight: number;
  isFeatured: boolean;
  sortOrder: number;
  images: string[];
  tags: string[];
  createdOnUtc: string;
}

// Order types
export interface OrderDto {
  id: string;
  customerName: string;
  customerEmail: string;
  shippingAddress: string;
  status: number;
  totalAmount: number;
  orderDate: string;
  createdOnUtc: string;
  items: OrderItemDto[];
}

export interface OrderItemDto {
  productSku: string;
  productName: string;
  unitPrice: number;
  quantity: number;
  totalPrice: number;
}

export interface CreateOrderRequest {
  customerName: string;
  customerEmail: string;
  shippingAddress: string;
  items: CreateOrderItemRequest[];
}

export interface CreateOrderItemRequest {
  productSku: string;
  quantity: number;
}

export interface CreateOrderResult {
  id: string;
}

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
