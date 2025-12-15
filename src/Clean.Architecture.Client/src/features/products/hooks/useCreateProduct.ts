import { useMutation, useQueryClient } from "@tanstack/react-query";
import { productsApi } from "../api/productsApi";
import { productKeys } from "./useProducts";
import type { CreateProductRequest } from "../types/types";

export function useCreateProduct() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (product: CreateProductRequest) =>
      productsApi.createProduct(product),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: productKeys.lists() });
    },
  });
}
