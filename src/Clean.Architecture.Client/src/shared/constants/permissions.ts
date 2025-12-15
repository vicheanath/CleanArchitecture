export const PERMISSIONS = {
  PRODUCTS_READ: "Products.Read",
  PRODUCTS_WRITE: "Products.Write",
  ORDERS_READ: "Orders.Read",
  ORDERS_CREATE: "Orders.Create",
  ORDERS_MANAGE: "Orders.Manage",
  INVENTORY_READ: "Inventory.Read",
  INVENTORY_WRITE: "Inventory.Write",
} as const;

export type Permission = (typeof PERMISSIONS)[keyof typeof PERMISSIONS];
