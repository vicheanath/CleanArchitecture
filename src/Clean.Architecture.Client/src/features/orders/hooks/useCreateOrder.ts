import { useMutation, useQueryClient } from "@tanstack/react-query";
import { ordersApi } from "../api/ordersApi";
import { orderKeys } from "./useOrders";
import type { CreateOrderRequest } from "../types/types";

export function useCreateOrder() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (order: CreateOrderRequest) => ordersApi.createOrder(order),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: orderKeys.lists() });
    },
  });
}
