// Components
export { InventoryManagementTable } from "./components/InventoryManagementTable";
export { StockManagement } from "./components/StockManagement";
export { AdjustStockDialog } from "./components/AdjustStockDialog";

// Pages
export { InventoryPage } from "./pages/InventoryPage";

// Hooks
export {
  useInventoryItem,
  useLowStockItems,
  inventoryKeys,
} from "./hooks/useInventory";
export { useCreateInventoryItem } from "./hooks/useCreateInventoryItem";
export { useAdjustInventoryStock } from "./hooks/useAdjustInventoryStock";
export { useReserveInventory } from "./hooks/useReserveInventory";

// API
export { inventoryApi } from "./api/inventoryApi";

// Types
export type {
  InventoryItemResponse,
  LowStockItemResponse,
  CreateInventoryItemRequest,
  AdjustInventoryStockRequest,
  ReserveInventoryRequest,
} from "./types/types";
