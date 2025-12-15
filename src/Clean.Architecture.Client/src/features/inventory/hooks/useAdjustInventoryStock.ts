import { useMutation, useQueryClient } from "@tanstack/react-query";
import { inventoryApi } from "../api/inventoryApi";
import { inventoryKeys } from "./useInventory";
import type { AdjustInventoryStockRequest } from "../types/types";

export function useAdjustInventoryStock() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      id,
      adjustment,
    }: {
      id: string;
      adjustment: AdjustInventoryStockRequest;
    }) => inventoryApi.adjustInventoryStock(id, adjustment),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({
        queryKey: inventoryKeys.item(variables.id),
      });
      queryClient.invalidateQueries({ queryKey: inventoryKeys.lowStock() });
    },
  });
}
