// Components
export { ProductManagement } from "./components/ProductManagement";
export { ProductManagementTable } from "./components/ProductManagementTable";
export { ProductCard } from "./components/ProductCard";
export { CreateProductDialog } from "./components/CreateProductDialog";
export { EditProductDialog } from "./components/EditProductDialog";

// Pages
export { ProductsPage } from "./pages/ProductsPage";
export { CreateProductPage } from "./pages/CreateProductPage";
export { ProductDetailsPage } from "./pages/ProductDetailsPage";

// Hooks
export { useProducts, useProduct, productKeys } from "./hooks/useProducts";
export { useCreateProduct } from "./hooks/useCreateProduct";
export { useUpdateProduct } from "./hooks/useUpdateProduct";
export { useDeleteProduct } from "./hooks/useDeleteProduct";

// API
export { productsApi } from "./api/productsApi";

// Types
export type {
  ProductDto,
  CreateProductRequest,
  CreateProductResult,
  UpdateProductRequest,
} from "./types/types";
