import { ApiClient } from "@/shared/lib/apiClient";
import type {
  InventoryItemResponse,
  LowStockItemResponse,
  CreateInventoryItemRequest,
  AdjustInventoryStockRequest,
  ReserveInventoryRequest,
} from "../types/types";

class InventoryApi extends ApiClient {
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
    const response = await this.request<{ value: string }>("/inventory", {
      method: "POST",
      body: JSON.stringify(item),
    });
    return "value" in response ? response.value : (response as string);
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
