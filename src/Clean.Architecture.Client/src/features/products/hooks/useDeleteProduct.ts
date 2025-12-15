import { useMutation, useQueryClient } from "@tanstack/react-query";
import { productsApi } from "../api/productsApi";
import { productKeys } from "./useProducts";

export function useDeleteProduct() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => productsApi.deleteProduct(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: productKeys.lists() });
    },
  });
}
