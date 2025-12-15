import {
  BrowserRouter as Router,
  Routes,
  Route,
  Navigate,
} from "react-router-dom";
import { QueryProvider } from "./shared/providers/QueryProvider";
import { AuthProvider, LoginPage, RegisterPage } from "./features/auth";
import { Layout } from "./shared/components/Layout";
import { DashboardPage } from "./features/dashboard";
import {
  ProductsPage,
  CreateProductPage,
  ProductDetailsPage,
} from "./features/products";
import { OrdersPage } from "./features/orders";
import { InventoryPage } from "./features/inventory";
import { ProtectedRoute } from "./shared/components/ProtectedRoute";
import { RequirePermission } from "./shared/components/RequirePermission";
import { PERMISSIONS } from "./shared/constants/permissions";

function App() {
  return (
    <QueryProvider>
      <Router>
        <AuthProvider>
          <Routes>
            {/* Public routes */}
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />

            {/* Protected routes */}
            <Route path="/" element={<Layout />}>
              <Route index element={<DashboardPage />} />
              <Route
                path="products"
                element={
                  <RequirePermission permission={PERMISSIONS.PRODUCTS_READ}>
                    <ProductsPage />
                  </RequirePermission>
                }
              />
              <Route
                path="products/create"
                element={
                  <RequirePermission permission={PERMISSIONS.PRODUCTS_WRITE}>
                    <CreateProductPage />
                  </RequirePermission>
                }
              />
              <Route
                path="products/:id"
                element={
                  <RequirePermission permission={PERMISSIONS.PRODUCTS_READ}>
                    <ProductDetailsPage />
                  </RequirePermission>
                }
              />
              <Route
                path="orders"
                element={
                  <RequirePermission permission={PERMISSIONS.ORDERS_READ}>
                    <OrdersPage />
                  </RequirePermission>
                }
              />
              <Route
                path="inventory"
                element={
                  <RequirePermission permission={PERMISSIONS.INVENTORY_READ}>
                    <InventoryPage />
                  </RequirePermission>
                }
              />
            </Route>

            {/* Default redirect */}
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </AuthProvider>
      </Router>
    </QueryProvider>
  );
}

export default App;
