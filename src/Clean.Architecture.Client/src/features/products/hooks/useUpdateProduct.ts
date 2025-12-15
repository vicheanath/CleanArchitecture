import { useMutation, useQueryClient } from "@tanstack/react-query";
import { productsApi } from "../api/productsApi";
import { productKeys } from "./useProducts";
import type { UpdateProductRequest } from "../types/types";

export function useUpdateProduct() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      id,
      product,
    }: {
      id: string;
      product: UpdateProductRequest;
    }) => productsApi.updateProduct(id, product),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: productKeys.lists() });
      queryClient.invalidateQueries({
        queryKey: productKeys.detail(variables.id),
      });
    },
  });
}
