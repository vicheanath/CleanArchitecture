import React from "react";
import {
  Package,
  ShoppingCart,
  AlertTriangle,
  DollarSign,
  BarChart3,
} from "lucide-react";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/shared/components/ui/card";
import { Button } from "@/shared/components/ui/button";
import {
  Tabs,
  TabsContent,
  TabsList,
  TabsTrigger,
} from "@/shared/components/ui/tabs";
import { useProducts } from "../../products/hooks/useProducts";
import { useOrders } from "../../orders/hooks/useOrders";
import { useLowStockItems } from "../../inventory/hooks/useInventory";
import { ProductManagement } from "../../products/components/ProductManagement";
import { StockManagement } from "../../inventory/components/StockManagement";

const DashboardPage: React.FC = () => {
  // Fetch dashboard data
  const { data: products = [] } = useProducts();
  const { data: orders = [] } = useOrders();
  const { data: lowStockItems = [] } = useLowStockItems();

  // Calculate dashboard metrics
  const metrics = {
    totalProducts: products.length,
    activeProducts: products.filter((p) => p.isActive).length,
    totalOrders: orders.length,
    totalRevenue: orders.reduce((sum, order) => sum + order.totalAmount, 0),
    lowStockAlerts: lowStockItems.filter(
      (item) => item.quantity < item.minimumStockLevel && item.quantity > 0
    ).length,
    outOfStockItems: lowStockItems.filter((item) => item.quantity === 0).length,
    avgOrderValue:
      orders.length > 0
        ? orders.reduce((sum, order) => sum + order.totalAmount, 0) /
          orders.length
        : 0,
    recentOrders: orders
      .sort(
        (a, b) =>
          new Date(b.createdOnUtc).getTime() -
          new Date(a.createdOnUtc).getTime()
      )
      .slice(0, 5),
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">
            E-commerce Dashboard
          </h1>
          <p className="text-muted-foreground">
            Welcome to your product and inventory management center
          </p>
        </div>
        <div className="text-sm text-muted-foreground">
          Last updated: {new Date().toLocaleDateString()}{" "}
          {new Date().toLocaleTimeString()}
        </div>
      </div>

      {/* Overview Cards */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Revenue</CardTitle>
            <DollarSign className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-green-600">
              ${metrics.totalRevenue.toFixed(2)}
            </div>
            <p className="text-xs text-muted-foreground">
              From {metrics.totalOrders} orders
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Products</CardTitle>
            <Package className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{metrics.totalProducts}</div>
            <p className="text-xs text-muted-foreground">
              {metrics.activeProducts} active
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Orders</CardTitle>
            <ShoppingCart className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{metrics.totalOrders}</div>
            <p className="text-xs text-muted-foreground">
              Avg: ${metrics.avgOrderValue.toFixed(2)}
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Stock Alerts</CardTitle>
            <AlertTriangle className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-red-600">
              {metrics.lowStockAlerts + metrics.outOfStockItems}
            </div>
            <p className="text-xs text-muted-foreground">
              {metrics.outOfStockItems} out of stock
            </p>
          </CardContent>
        </Card>
      </div>

      {/* Alert Section */}
      {(metrics.lowStockAlerts > 0 || metrics.outOfStockItems > 0) && (
        <Card className="border-yellow-200 bg-yellow-50">
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-yellow-800">
              <AlertTriangle className="h-5 w-5" />
              Inventory Alerts
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-2">
              {metrics.outOfStockItems > 0 && (
                <div className="flex items-center justify-between">
                  <span className="text-red-600 font-medium">
                    {metrics.outOfStockItems} items are out of stock
                  </span>
                  <Button variant="outline" size="sm">
                    View Details
                  </Button>
                </div>
              )}
              {metrics.lowStockAlerts > 0 && (
                <div className="flex items-center justify-between">
                  <span className="text-yellow-600 font-medium">
                    {metrics.lowStockAlerts} items are running low
                  </span>
                  <Button variant="outline" size="sm">
                    Restock Now
                  </Button>
                </div>
              )}
            </div>
          </CardContent>
        </Card>
      )}

      {/* Recent Activity */}
      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <ShoppingCart className="h-5 w-5" />
              Recent Orders
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {metrics.recentOrders.length > 0 ? (
                metrics.recentOrders.map((order) => (
                  <div
                    key={order.id}
                    className="flex items-center justify-between"
                  >
                    <div>
                      <div className="font-medium">{order.customerName}</div>
                      <div className="text-sm text-muted-foreground">
                        {order.items.length} items •{" "}
                        {new Date(order.createdOnUtc).toLocaleDateString()}
                      </div>
                    </div>
                    <div className="text-right">
                      <div className="font-medium">
                        ${order.totalAmount.toFixed(2)}
                      </div>
                      <div className="text-sm text-muted-foreground">
                        Status: {order.status === 1 ? "Confirmed" : "Pending"}
                      </div>
                    </div>
                  </div>
                ))
              ) : (
                <p className="text-muted-foreground text-center py-4">
                  No orders yet
                </p>
              )}
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <BarChart3 className="h-5 w-5" />
              Quick Stats
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              <div className="flex items-center justify-between">
                <span className="text-muted-foreground">
                  Products Created Today
                </span>
                <span className="font-medium">
                  {
                    products.filter(
                      (p) =>
                        new Date(p.createdOnUtc).toDateString() ===
                        new Date().toDateString()
                    ).length
                  }
                </span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-muted-foreground">Orders Today</span>
                <span className="font-medium">
                  {
                    orders.filter(
                      (o) =>
                        new Date(o.createdOnUtc).toDateString() ===
                        new Date().toDateString()
                    ).length
                  }
                </span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-muted-foreground">Revenue Today</span>
                <span className="font-medium text-green-600">
                  $
                  {orders
                    .filter(
                      (o) =>
                        new Date(o.createdOnUtc).toDateString() ===
                        new Date().toDateString()
                    )
                    .reduce((sum, o) => sum + o.totalAmount, 0)
                    .toFixed(2)}
                </span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-muted-foreground">Inventory Items</span>
                <span className="font-medium">{lowStockItems.length}</span>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Management Tabs */}
      <Card>
        <CardHeader>
          <CardTitle>Management Tools</CardTitle>
        </CardHeader>
        <CardContent>
          <Tabs defaultValue="products" className="w-full">
            <TabsList className="grid w-full grid-cols-2">
              <TabsTrigger value="products" className="flex items-center gap-2">
                <Package className="h-4 w-4" />
                Product Management
              </TabsTrigger>
              <TabsTrigger
                value="inventory"
                className="flex items-center gap-2"
              >
                <BarChart3 className="h-4 w-4" />
                Stock Management
              </TabsTrigger>
            </TabsList>

            <TabsContent value="products" className="mt-6">
              <ProductManagement />
            </TabsContent>

            <TabsContent value="inventory" className="mt-6">
              <StockManagement />
            </TabsContent>
          </Tabs>
        </CardContent>
      </Card>
    </div>
  );
};

export default DashboardPage;
