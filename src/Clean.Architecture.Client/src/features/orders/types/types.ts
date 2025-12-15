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
