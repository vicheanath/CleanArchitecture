import React, { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import {
  Plus,
  ShoppingCart,
  User,
  Mail,
  MapPin,
  Calendar,
  DollarSign,
  Package,
  Search,
  Filter,
} from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/shared/components/ui/card";
import { Input } from "@/shared/components/ui/input";
import { Label } from "@/shared/components/ui/label";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/shared/components/ui/dialog";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/ui/select";
import { Textarea } from "@/shared/components/ui/textarea";
import { useOrders } from "../hooks/useOrders";
import { useCreateOrder } from "../hooks/useCreateOrder";
import { productsApi } from "../../products/api/productsApi";
import type { OrderDto, CreateOrderRequest } from "../types/types";
import type { ProductDto } from "../../products/types/types";

export const OrderManagement: React.FC = () => {
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState<string>("all");
  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false);

  const {
    data: orders = [],
    isLoading: ordersLoading,
    error: ordersError,
  } = useOrders();
  const createOrderMutation = useCreateOrder();

  // Query for products (needed for creating orders)
  const { data: products = [] } = useQuery({
    queryKey: ["products"],
    queryFn: () => productsApi.getProducts(),
  });

  // Filter orders
  const filteredOrders = orders.filter((order) => {
    const matchesSearch =
      order.customerName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      order.customerEmail.toLowerCase().includes(searchTerm.toLowerCase()) ||
      order.id.toLowerCase().includes(searchTerm.toLowerCase());

    const matchesStatus =
      statusFilter === "all" ||
      (statusFilter === "confirmed" && order.status === 1) ||
      (statusFilter === "pending" && order.status === 0);

    return matchesSearch && matchesStatus;
  });

  // Calculate stats
  const stats = {
    totalOrders: orders.length,
    confirmedOrders: orders.filter((o) => o.status === 1).length,
    pendingOrders: orders.filter((o) => o.status === 0).length,
    totalRevenue: orders.reduce((sum, order) => sum + order.totalAmount, 0),
    avgOrderValue:
      orders.length > 0
        ? orders.reduce((sum, order) => sum + order.totalAmount, 0) /
          orders.length
        : 0,
  };

  const handleCreateOrder = (data: CreateOrderRequest) => {
    createOrderMutation.mutate(data, {
      onSuccess: () => {
        setIsCreateDialogOpen(false);
      },
    });
  };

  if (ordersLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="text-lg">Loading orders...</div>
      </div>
    );
  }

  if (ordersError) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="text-lg text-red-600">
          Error loading orders: {ordersError.message}
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">
            Order Management
          </h1>
          <p className="text-muted-foreground">
            Manage customer orders and track sales
          </p>
        </div>
        <CreateOrderDialog
          isOpen={isCreateDialogOpen}
          onOpenChange={setIsCreateDialogOpen}
          onSubmit={handleCreateOrder}
          isLoading={createOrderMutation.isPending}
          products={products}
        />
      </div>

      {/* Stats Cards */}
      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Orders</CardTitle>
            <ShoppingCart className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.totalOrders}</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Confirmed</CardTitle>
            <Package className="h-4 w-4 text-green-600" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-green-600">
              {stats.confirmedOrders}
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Pending</CardTitle>
            <Package className="h-4 w-4 text-yellow-600" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-yellow-600">
              {stats.pendingOrders}
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Revenue</CardTitle>
            <DollarSign className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-green-600">
              ${stats.totalRevenue.toFixed(2)}
            </div>
            <p className="text-xs text-muted-foreground">
              Avg: ${stats.avgOrderValue.toFixed(2)}
            </p>
          </CardContent>
        </Card>
      </div>

      {/* Filters */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Filter className="h-5 w-5" />
            Filters
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex flex-col sm:flex-row gap-4">
            <div className="flex-1">
              <Label htmlFor="search">Search Orders</Label>
              <div className="relative">
                <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                <Input
                  id="search"
                  placeholder="Search by customer name, email, or order ID..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="pl-10"
                />
              </div>
            </div>
            <div className="w-full sm:w-64">
              <Label htmlFor="status">Order Status</Label>
              <Select value={statusFilter} onValueChange={setStatusFilter}>
                <SelectTrigger>
                  <SelectValue placeholder="Filter by status" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Orders</SelectItem>
                  <SelectItem value="confirmed">Confirmed</SelectItem>
                  <SelectItem value="pending">Pending</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Orders List */}
      <div className="space-y-4">
        {filteredOrders.map((order) => (
          <OrderCard key={order.id} order={order} />
        ))}
      </div>

      {filteredOrders.length === 0 && (
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-12">
            <ShoppingCart className="h-12 w-12 text-muted-foreground mb-4" />
            <h3 className="text-lg font-semibold mb-2">No orders found</h3>
            <p className="text-muted-foreground text-center">
              {searchTerm || statusFilter !== "all"
                ? "Try adjusting your filters to see more orders."
                : "No orders have been placed yet."}
            </p>
          </CardContent>
        </Card>
      )}
    </div>
  );
};

interface OrderCardProps {
  order: OrderDto;
}

const OrderCard: React.FC<OrderCardProps> = ({ order }) => {
  const getStatusColor = () => {
    return order.status === 1
      ? "bg-green-100 text-green-800 border-green-200"
      : "bg-yellow-100 text-yellow-800 border-yellow-200";
  };

  const getStatusText = () => {
    return order.status === 1 ? "Confirmed" : "Pending";
  };

  return (
    <Card className="hover:shadow-md transition-shadow">
      <CardContent className="p-6">
        <div className="flex justify-between items-start mb-4">
          <div className="space-y-2">
            <div className="flex items-center gap-3">
              <h3 className="text-lg font-semibold">
                Order #{order.id.slice(-8)}
              </h3>
              <span
                className={`px-3 py-1 text-xs font-medium rounded-full border ${getStatusColor()}`}
              >
                {getStatusText()}
              </span>
            </div>
            <div className="flex items-center gap-4 text-sm text-muted-foreground">
              <div className="flex items-center gap-1">
                <User className="h-4 w-4" />
                {order.customerName}
              </div>
              <div className="flex items-center gap-1">
                <Mail className="h-4 w-4" />
                {order.customerEmail}
              </div>
              <div className="flex items-center gap-1">
                <Calendar className="h-4 w-4" />
                {new Date(order.createdOnUtc).toLocaleDateString()}
              </div>
            </div>
          </div>
          <div className="text-right">
            <div className="text-2xl font-bold text-green-600">
              ${order.totalAmount.toFixed(2)}
            </div>
            <div className="text-sm text-muted-foreground">
              {order.items.length} item{order.items.length !== 1 ? "s" : ""}
            </div>
          </div>
        </div>

        <div className="space-y-2">
          <div className="flex items-center gap-1 text-sm text-muted-foreground">
            <MapPin className="h-4 w-4" />
            <span className="font-medium">Shipping Address:</span>
            {order.shippingAddress}
          </div>

          <div className="mt-4">
            <h4 className="font-medium mb-2">Order Items:</h4>
            <div className="space-y-1">
              {order.items.map((item, index) => (
                <div
                  key={index}
                  className="flex justify-between items-center text-sm"
                >
                  <span>
                    {item.productName} ({item.productSku}) × {item.quantity}
                  </span>
                  <span className="font-medium">
                    ${item.totalPrice.toFixed(2)}
                  </span>
                </div>
              ))}
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  );
};

interface CreateOrderDialogProps {
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
  onSubmit: (data: CreateOrderRequest) => void;
  isLoading: boolean;
  products: ProductDto[];
}

const CreateOrderDialog: React.FC<CreateOrderDialogProps> = ({
  isOpen,
  onOpenChange,
  onSubmit,
  isLoading,
  products,
}) => {
  const [formData, setFormData] = useState<CreateOrderRequest>({
    customerName: "",
    customerEmail: "",
    shippingAddress: "",
    items: [],
  });

  const [newItem, setNewItem] = useState({
    productSku: "",
    quantity: 1,
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (formData.items.length === 0) {
      alert("Please add at least one item to the order.");
      return;
    }
    onSubmit(formData);
  };

  const addItem = () => {
    if (!newItem.productSku || newItem.quantity < 1) return;

    setFormData({
      ...formData,
      items: [...formData.items, { ...newItem }],
    });
    setNewItem({ productSku: "", quantity: 1 });
  };

  const removeItem = (index: number) => {
    setFormData({
      ...formData,
      items: formData.items.filter((_, i) => i !== index),
    });
  };

  const resetForm = () => {
    setFormData({
      customerName: "",
      customerEmail: "",
      shippingAddress: "",
      items: [],
    });
    setNewItem({ productSku: "", quantity: 1 });
  };

  return (
    <Dialog
      open={isOpen}
      onOpenChange={(open) => {
        onOpenChange(open);
        if (!open) resetForm();
      }}
    >
      <DialogTrigger asChild>
        <Button>
          <Plus className="h-4 w-4 mr-2" />
          Create Order
        </Button>
      </DialogTrigger>
      <DialogContent className="max-w-2xl max-h-[80vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>Create New Order</DialogTitle>
          <DialogDescription>
            Create a new customer order with multiple products.
          </DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="customerName">Customer Name</Label>
              <Input
                id="customerName"
                placeholder="John Doe"
                value={formData.customerName}
                onChange={(e) =>
                  setFormData({ ...formData, customerName: e.target.value })
                }
                required
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="customerEmail">Customer Email</Label>
              <Input
                id="customerEmail"
                type="email"
                placeholder="john@example.com"
                value={formData.customerEmail}
                onChange={(e) =>
                  setFormData({ ...formData, customerEmail: e.target.value })
                }
                required
              />
            </div>
          </div>

          <div className="space-y-2">
            <Label htmlFor="shippingAddress">Shipping Address</Label>
            <Textarea
              id="shippingAddress"
              placeholder="123 Main St, City, State 12345"
              value={formData.shippingAddress}
              onChange={(e) =>
                setFormData({ ...formData, shippingAddress: e.target.value })
              }
              required
            />
          </div>

          <div className="space-y-4">
            <h4 className="font-medium">Order Items</h4>

            {/* Add Item Section */}
            <div className="border rounded-lg p-4 space-y-4">
              <h5 className="font-medium text-sm">Add Item</h5>
              <div className="grid grid-cols-3 gap-2">
                <Select
                  value={newItem.productSku}
                  onValueChange={(value) =>
                    setNewItem({ ...newItem, productSku: value })
                  }
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Select product" />
                  </SelectTrigger>
                  <SelectContent>
                    {products.map((product) => (
                      <SelectItem key={product.sku} value={product.sku}>
                        {product.name} ({product.sku}) - ${product.price}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <Input
                  type="number"
                  min="1"
                  placeholder="Qty"
                  value={newItem.quantity}
                  onChange={(e) =>
                    setNewItem({
                      ...newItem,
                      quantity: parseInt(e.target.value) || 1,
                    })
                  }
                />
                <Button type="button" onClick={addItem} size="sm">
                  Add
                </Button>
              </div>
            </div>

            {/* Items List */}
            {formData.items.length > 0 && (
              <div className="space-y-2">
                <h5 className="font-medium text-sm">Items in Order</h5>
                {formData.items.map((item, index) => {
                  const product = products.find(
                    (p) => p.sku === item.productSku
                  );
                  return (
                    <div
                      key={index}
                      className="flex justify-between items-center bg-muted p-3 rounded"
                    >
                      <span>
                        {product?.name || item.productSku} × {item.quantity}
                      </span>
                      <div className="flex items-center gap-2">
                        <span className="font-medium">
                          ${((product?.price || 0) * item.quantity).toFixed(2)}
                        </span>
                        <Button
                          type="button"
                          variant="destructive"
                          size="sm"
                          onClick={() => removeItem(index)}
                        >
                          Remove
                        </Button>
                      </div>
                    </div>
                  );
                })}
                <div className="text-right font-semibold">
                  Total: $
                  {formData.items
                    .reduce((sum, item) => {
                      const product = products.find(
                        (p) => p.sku === item.productSku
                      );
                      return sum + (product?.price || 0) * item.quantity;
                    }, 0)
                    .toFixed(2)}
                </div>
              </div>
            )}
          </div>

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
            >
              Cancel
            </Button>
            <Button
              type="submit"
              disabled={isLoading || formData.items.length === 0}
            >
              {isLoading ? "Creating..." : "Create Order"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
};
