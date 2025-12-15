import type {
  InventoryItemResponse,
  LowStockItemResponse,
  CreateInventoryItemRequest,
  AdjustInventoryStockRequest,
  ReserveInventoryRequest,
} from "../types/types";

const API_BASE_URL = "http://localhost:5105/api";

class InventoryApi {
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

export const inventoryApi = new InventoryApi();
