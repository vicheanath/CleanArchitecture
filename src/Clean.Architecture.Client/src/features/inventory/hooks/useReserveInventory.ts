import { useMutation, useQueryClient } from "@tanstack/react-query";
import { inventoryApi } from "../api/inventoryApi";
import { inventoryKeys } from "./useInventory";
import type { ReserveInventoryRequest } from "../types/types";

export function useReserveInventory() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      id,
      reservation,
    }: {
      id: string;
      reservation: ReserveInventoryRequest;
    }) => inventoryApi.reserveInventory(id, reservation),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({
        queryKey: inventoryKeys.item(variables.id),
      });
    },
  });
}
