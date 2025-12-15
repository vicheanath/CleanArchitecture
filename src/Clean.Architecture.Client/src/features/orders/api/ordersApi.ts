import type {
  OrderDto,
  CreateOrderRequest,
  CreateOrderResult,
} from "../types/types";

const API_BASE_URL = "http://localhost:5105/api";

class OrdersApi {
  private async request<T>(
    endpoint: string,
    options?: RequestInit
  ): Promise<T> {
    try {
      const response = await fetch(`${API_BASE_URL}${endpoint}`, {
        headers: {
          "Content-Type": "application/json",
          ...options?.headers,
        },
        ...options,
      });

      if (!response.ok) {
        if (response.status === 404) {
          throw new Error(`Resource not found: ${endpoint}`);
        }
        if (response.status === 400) {
          throw new Error(`Bad request: ${response.statusText}`);
        }
        if (response.status >= 500) {
          throw new Error(
            `Server error: ${response.status} ${response.statusText}`
          );
        }
        throw new Error(`API Error: ${response.status} ${response.statusText}`);
      }

      const contentType = response.headers.get("content-type");
      if (contentType && contentType.includes("application/json")) {
        return response.json();
      } else {
        return {} as T;
      }
    } catch (error) {
      if (error instanceof TypeError && error.message.includes("fetch")) {
        throw new Error(
          "Network error: Unable to connect to the server. Please check if the API is running."
        );
      }
      throw error;
    }
  }

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
    return this.request<CreateOrderResult>("/orders", {
      method: "POST",
      body: JSON.stringify(order),
    });
  }
}

export const ordersApi = new OrdersApi();
