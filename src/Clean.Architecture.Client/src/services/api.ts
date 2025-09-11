import type {
  ProductDto,
  CreateProductRequest,
  CreateProductResult,
  OrderDto,
  CreateOrderRequest,
  CreateOrderResult,
  InventoryItemResponse,
  LowStockItemResponse,
  CreateInventoryItemRequest,
  AdjustInventoryStockRequest,
  ReserveInventoryRequest,
} from "@/types/api";

const API_BASE_URL = "http://localhost:5105/api";

class ApiService {
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
        // Handle different error status codes
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

      // Handle empty responses
      const contentType = response.headers.get("content-type");
      if (contentType && contentType.includes("application/json")) {
        return response.json();
      } else {
        // For responses that don't return JSON (like some POST operations)
        return {} as T;
      }
    } catch (error) {
      // Handle network errors
      if (error instanceof TypeError && error.message.includes("fetch")) {
        throw new Error(
          "Network error: Unable to connect to the server. Please check if the API is running."
        );
      }
      // Re-throw other errors
      throw error;
    }
  }

  // Product API
  async getProducts(): Promise<ProductDto[]> {
    try {
      const products = await this.request<ProductDto[]>("/products");
      // Ensure we always return an array
      return Array.isArray(products) ? products : [];
    } catch (error) {
      console.error("Error fetching products:", error);
      throw error;
    }
  }

  async getProduct(id: string): Promise<ProductDto> {
    if (!id) {
      throw new Error("Product ID is required");
    }
    return this.request<ProductDto>(`/products/${id}`);
  }

  async createProduct(
    product: CreateProductRequest
  ): Promise<CreateProductResult> {
    if (!product || !product.sku || !product.name || !product.price) {
      throw new Error("Invalid product data: missing required fields");
    }
    return this.request<CreateProductResult>("/products", {
      method: "POST",
      body: JSON.stringify(product),
    });
  }

  async deleteProduct(id: string): Promise<void> {
    if (!id) {
      throw new Error("Product ID is required");
    }
    await this.request(`/products/${id}`, {
      method: "DELETE",
    });
  }

  // Order API
  async getOrders(): Promise<OrderDto[]> {
    try {
      const orders = await this.request<OrderDto[]>("/orders");
      // Ensure we always return an array
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

  // Inventory API
  async getInventoryItem(id: string): Promise<InventoryItemResponse> {
    if (!id) {
      throw new Error("Inventory item ID is required");
    }
    return this.request<InventoryItemResponse>(`/inventory/${id}`);
  }

  async getLowStockItems(): Promise<LowStockItemResponse[]> {
    try {
      const items = await this.request<LowStockItemResponse[]>(
        "/inventory/low-stock"
      );
      // Ensure we always return an array
      return Array.isArray(items) ? items : [];
    } catch (error) {
      console.error("Error fetching low stock items:", error);
      throw error;
    }
  }

  async createInventoryItem(item: CreateInventoryItemRequest): Promise<string> {
    if (!item || !item.productSku) {
      throw new Error("Invalid inventory item data: missing required fields");
    }
    return this.request<string>("/inventory", {
      method: "POST",
      body: JSON.stringify(item),
    });
  }

  async adjustInventoryStock(
    id: string,
    adjustment: AdjustInventoryStockRequest
  ): Promise<void> {
    if (!id) {
      throw new Error("Inventory item ID is required");
    }
    if (!adjustment || typeof adjustment.quantityChange !== "number") {
      throw new Error(
        "Invalid adjustment data: missing or invalid quantityChange"
      );
    }
    await this.request(`/inventory/${id}/adjust-stock`, {
      method: "PATCH",
      body: JSON.stringify(adjustment),
    });
  }

  async reserveInventory(
    id: string,
    reservation: ReserveInventoryRequest
  ): Promise<void> {
    if (!id) {
      throw new Error("Inventory item ID is required");
    }
    if (!reservation || typeof reservation.quantity !== "number") {
      throw new Error("Invalid reservation data: missing or invalid quantity");
    }
    await this.request(`/inventory/${id}/reservations`, {
      method: "POST",
      body: JSON.stringify(reservation),
    });
  }
}

export const apiService = new ApiService();
