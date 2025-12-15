import { useQuery } from "@tanstack/react-query";
import { productsApi } from "../api/productsApi";

// Query Keys
export const productKeys = {
  all: ["products"] as const,
  lists: () => [...productKeys.all, "list"] as const,
  list: (filters: string) => [...productKeys.lists(), { filters }] as const,
  details: () => [...productKeys.all, "detail"] as const,
  detail: (id: string) => [...productKeys.details(), id] as const,
};

// Queries
export function useProducts() {
  return useQuery({
    queryKey: productKeys.lists(),
    queryFn: () => productsApi.getProducts(),
  });
}

export function useProduct(id: string) {
  return useQuery({
    queryKey: productKeys.detail(id),
    queryFn: () => productsApi.getProduct(id),
    enabled: !!id,
  });
}
