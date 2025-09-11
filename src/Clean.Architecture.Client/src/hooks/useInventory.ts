import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { apiService } from "@/services/api";
import type {
  CreateInventoryItemRequest,
  AdjustInventoryStockRequest,
  ReserveInventoryRequest,
} from "@/types/api";

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
    queryFn: () => apiService.getInventoryItem(id),
    enabled: !!id,
  });
}

export function useLowStockItems() {
  return useQuery({
    queryKey: inventoryKeys.lowStock(),
    queryFn: () => apiService.getLowStockItems(),
  });
}

// Mutations
export function useCreateInventoryItem() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (item: CreateInventoryItemRequest) =>
      apiService.createInventoryItem(item),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: inventoryKeys.lowStock() });
    },
  });
}

export function useAdjustInventoryStock() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      id,
      adjustment,
    }: {
      id: string;
      adjustment: AdjustInventoryStockRequest;
    }) => apiService.adjustInventoryStock(id, adjustment),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({
        queryKey: inventoryKeys.item(variables.id),
      });
      queryClient.invalidateQueries({ queryKey: inventoryKeys.lowStock() });
    },
  });
}

export function useReserveInventory() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      id,
      reservation,
    }: {
      id: string;
      reservation: ReserveInventoryRequest;
    }) => apiService.reserveInventory(id, reservation),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({
        queryKey: inventoryKeys.item(variables.id),
      });
    },
  });
}
