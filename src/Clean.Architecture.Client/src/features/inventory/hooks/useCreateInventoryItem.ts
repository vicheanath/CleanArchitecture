import { useMutation, useQueryClient } from "@tanstack/react-query";
import { inventoryApi } from "../api/inventoryApi";
import { inventoryKeys } from "./useInventory";
import type { CreateInventoryItemRequest } from "../types/types";

export function useCreateInventoryItem() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (item: CreateInventoryItemRequest) =>
      inventoryApi.createInventoryItem(item),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: inventoryKeys.lowStock() });
    },
  });
}
