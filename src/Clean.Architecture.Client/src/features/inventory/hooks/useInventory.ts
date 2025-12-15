import { useQuery } from "@tanstack/react-query";
import { inventoryApi } from "../api/inventoryApi";

// Query Keys
export const inventoryKeys = {
  all: ["inventory"] as const,
  items: () => [...inventoryKeys.all, "item"] as const,
  item: (id: string) => [...inventoryKeys.items(), id] as const,
  lowStock: () => [...inventoryKeys.all, "low-stock"] as const,
};

// Queries
export function useInventoryItem(id: string) {
  return useQuery({
    queryKey: inventoryKeys.item(id),
    queryFn: () => inventoryApi.getInventoryItem(id),
    enabled: !!id,
  });
}

export function useLowStockItems() {
  return useQuery({
    queryKey: inventoryKeys.lowStock(),
    queryFn: () => inventoryApi.getLowStockItems(),
    refetchInterval: 30000, // Refresh every 30 seconds
  });
}
