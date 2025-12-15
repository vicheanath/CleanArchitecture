import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import { QueryProvider } from "./shared/providers/QueryProvider";
import { Layout } from "./shared/components/Layout";
import { DashboardPage } from "./features/dashboard";
import {
  ProductsPage,
  CreateProductPage,
  ProductDetailsPage,
} from "./features/products";
import { OrdersPage } from "./features/orders";
import { InventoryPage } from "./features/inventory";

function App() {
  return (
    <QueryProvider>
      <Router>
        <Routes>
          <Route path="/" element={<Layout />}>
            <Route index element={<DashboardPage />} />
            <Route path="products" element={<ProductsPage />} />
            <Route path="products/create" element={<CreateProductPage />} />
            <Route path="products/:id" element={<ProductDetailsPage />} />
            <Route path="orders" element={<OrdersPage />} />
            <Route path="inventory" element={<InventoryPage />} />
          </Route>
        </Routes>
      </Router>
    </QueryProvider>
  );
}

export default App;
