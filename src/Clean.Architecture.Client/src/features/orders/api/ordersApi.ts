import { ApiClient } from "@/shared/lib/apiClient";
import type {
  OrderDto,
  CreateOrderRequest,
  CreateOrderResult,
} from "../types/types";

class OrdersApi extends ApiClient {
  async getOrders(): Promise<OrderDto[]> {
    try {
      const orders = await this.request<OrderDto[]>("/orders");
      return Array.isArray(orders) ? orders : [];
    } catch (error) {
      console.error("Error fetching orders:", error);
      throw error;
    }
  }

  async getOrder(id: string): Promise<OrderDto> {
    if (!id) {
      throw new Error("Order ID is required");
    }
    return this.request<OrderDto>(`/orders/${id}`);
  }

  async createOrder(order: CreateOrderRequest): Promise<CreateOrderResult> {
    if (
      !order ||
      !order.customerName ||
      !order.customerEmail ||
      !order.items?.length
    ) {
      throw new Error("Invalid order data: missing required fields");
    }
    const response = await this.request<{ value: CreateOrderResult }>(
      "/orders",
      {
        method: "POST",
        body: JSON.stringify(order),
      }
    );
    return "value" in response
      ? response.value
      : (response as CreateOrderResult);
  }
}

export const ordersApi = new OrdersApi();
