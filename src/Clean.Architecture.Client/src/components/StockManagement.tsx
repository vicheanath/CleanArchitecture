import React, { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  Package,
  AlertTriangle,
  Plus,
  Minus,
  Search,
  Filter,
  TrendingUp,
  TrendingDown,
  RefreshCw,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import { apiService } from "@/services/api";
import type {
  LowStockItemResponse,
  AdjustInventoryStockRequest,
} from "@/types/api";

const StockManagement: React.FC = () => {
  const [searchTerm, setSearchTerm] = useState("");
  const [stockFilter, setStockFilter] = useState<string>("all");
  const [selectedInventoryItem, setSelectedInventoryItem] =
    useState<LowStockItemResponse | null>(null);

  const queryClient = useQueryClient();

  // Query for low stock items (this returns all inventory items currently)
  const {
    data: inventoryItems = [],
    isLoading,
    error,
    refetch,
  } = useQuery({
    queryKey: ["lowStockItems"],
    queryFn: () => apiService.getLowStockItems(),
    refetchInterval: 30000, // Refresh every 30 seconds
  });

  // Adjust stock mutation
  const adjustStockMutation = useMutation({
    mutationFn: ({
      id,
      adjustment,
    }: {
      id: string;
      adjustment: AdjustInventoryStockRequest;
    }) => apiService.adjustInventoryStock(id, adjustment),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["lowStockItems"] });
      setSelectedInventoryItem(null);
    },
  });

  // Filter inventory items
  const filteredItems = inventoryItems.filter((item) => {
    const matchesSearch = item.productSku
      .toLowerCase()
      .includes(searchTerm.toLowerCase());

    const matchesFilter =
      stockFilter === "all" ||
      (stockFilter === "low" && item.quantity < item.minimumStockLevel) ||
      (stockFilter === "out" && item.quantity === 0) ||
      (stockFilter === "adequate" && item.quantity >= item.minimumStockLevel);

    return matchesSearch && matchesFilter;
  });

  // Calculate stats
  const stats = {
    totalItems: inventoryItems.length,
    lowStock: inventoryItems.filter(
      (item) => item.quantity < item.minimumStockLevel && item.quantity > 0
    ).length,
    outOfStock: inventoryItems.filter((item) => item.quantity === 0).length,
    adequateStock: inventoryItems.filter(
      (item) => item.quantity >= item.minimumStockLevel
    ).length,
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="text-lg">Loading inventory...</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="text-lg text-red-600">
          Error loading inventory: {error.message}
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
            Stock Management
          </h1>
          <p className="text-muted-foreground">
            Monitor and manage your inventory levels
          </p>
        </div>
        <Button onClick={() => refetch()} variant="outline">
          <RefreshCw className="h-4 w-4 mr-2" />
          Refresh
        </Button>
      </div>

      {/* Stats Cards */}
      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Items</CardTitle>
            <Package className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.totalItems}</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">
              Adequate Stock
            </CardTitle>
            <TrendingUp className="h-4 w-4 text-green-600" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-green-600">
              {stats.adequateStock}
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Low Stock</CardTitle>
            <AlertTriangle className="h-4 w-4 text-yellow-600" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-yellow-600">
              {stats.lowStock}
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Out of Stock</CardTitle>
            <TrendingDown className="h-4 w-4 text-red-600" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-red-600">
              {stats.outOfStock}
            </div>
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
              <Label htmlFor="search">Search by SKU</Label>
              <div className="relative">
                <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                <Input
                  id="search"
                  placeholder="Search by product SKU..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="pl-10"
                />
              </div>
            </div>
            <div className="w-full sm:w-64">
              <Label htmlFor="stock-filter">Stock Level</Label>
              <Select value={stockFilter} onValueChange={setStockFilter}>
                <SelectTrigger>
                  <SelectValue placeholder="Filter by stock level" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Items</SelectItem>
                  <SelectItem value="adequate">Adequate Stock</SelectItem>
                  <SelectItem value="low">Low Stock</SelectItem>
                  <SelectItem value="out">Out of Stock</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Inventory Items */}
      <div className="grid gap-4">
        {filteredItems.map((item) => (
          <InventoryItemCard
            key={item.id}
            item={item}
            onAdjustStock={() => setSelectedInventoryItem(item)}
          />
        ))}
      </div>

      {filteredItems.length === 0 && (
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-12">
            <Package className="h-12 w-12 text-muted-foreground mb-4" />
            <h3 className="text-lg font-semibold mb-2">
              No inventory items found
            </h3>
            <p className="text-muted-foreground text-center">
              {searchTerm || stockFilter !== "all"
                ? "Try adjusting your filters to see more items."
                : "No inventory items are available."}
            </p>
          </CardContent>
        </Card>
      )}

      {/* Adjust Stock Dialog */}
      {selectedInventoryItem && (
        <AdjustStockDialog
          item={selectedInventoryItem}
          isOpen={!!selectedInventoryItem}
          onOpenChange={(open) => !open && setSelectedInventoryItem(null)}
          onAdjust={(adjustment) => {
            adjustStockMutation.mutate({
              id: selectedInventoryItem.id,
              adjustment,
            });
          }}
          isLoading={adjustStockMutation.isPending}
        />
      )}
    </div>
  );
};

interface InventoryItemCardProps {
  item: LowStockItemResponse;
  onAdjustStock: () => void;
}

const InventoryItemCard: React.FC<InventoryItemCardProps> = ({
  item,
  onAdjustStock,
}) => {
  const getStatusColor = () => {
    if (item.quantity === 0) return "bg-red-100 text-red-800 border-red-200";
    if (item.quantity < item.minimumStockLevel)
      return "bg-yellow-100 text-yellow-800 border-yellow-200";
    return "bg-green-100 text-green-800 border-green-200";
  };

  const getStatusText = () => {
    if (item.quantity === 0) return "Out of Stock";
    if (item.quantity < item.minimumStockLevel) return "Low Stock";
    return "In Stock";
  };

  const stockPercentage = Math.max(
    0,
    Math.min(100, (item.quantity / (item.minimumStockLevel * 2)) * 100)
  );

  return (
    <Card className="hover:shadow-md transition-shadow">
      <CardContent className="p-6">
        <div className="flex items-center justify-between">
          <div className="space-y-2">
            <div className="flex items-center gap-3">
              <h3 className="text-lg font-semibold">{item.productSku}</h3>
              <span
                className={`px-3 py-1 text-xs font-medium rounded-full border ${getStatusColor()}`}
              >
                {getStatusText()}
              </span>
            </div>
            <div className="grid grid-cols-3 gap-4 text-sm">
              <div>
                <span className="text-muted-foreground">Current Stock:</span>
                <div className="font-semibold text-lg">{item.quantity}</div>
              </div>
              <div>
                <span className="text-muted-foreground">Minimum Level:</span>
                <div className="font-semibold text-lg">
                  {item.minimumStockLevel}
                </div>
              </div>
              <div>
                <span className="text-muted-foreground">Stock Deficit:</span>
                <div className="font-semibold text-lg text-red-600">
                  {item.stockDeficit > 0 ? `-${item.stockDeficit}` : "0"}
                </div>
              </div>
            </div>

            {/* Stock Level Bar */}
            <div className="w-full">
              <div className="flex justify-between text-xs text-muted-foreground mb-1">
                <span>Stock Level</span>
                <span>{stockPercentage.toFixed(0)}%</span>
              </div>
              <div className="w-full bg-gray-200 rounded-full h-2">
                <div
                  className={`h-2 rounded-full transition-all ${
                    item.quantity === 0
                      ? "bg-red-500"
                      : item.quantity < item.minimumStockLevel
                      ? "bg-yellow-500"
                      : "bg-green-500"
                  }`}
                  style={{ width: `${stockPercentage}%` }}
                />
              </div>
            </div>
          </div>

          <Button onClick={onAdjustStock} variant="outline">
            Adjust Stock
          </Button>
        </div>
      </CardContent>
    </Card>
  );
};

interface AdjustStockDialogProps {
  item: LowStockItemResponse;
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
  onAdjust: (adjustment: AdjustInventoryStockRequest) => void;
  isLoading: boolean;
}

const AdjustStockDialog: React.FC<AdjustStockDialogProps> = ({
  item,
  isOpen,
  onOpenChange,
  onAdjust,
  isLoading,
}) => {
  const [adjustmentType, setAdjustmentType] = useState<"increase" | "decrease">(
    "increase"
  );
  const [quantity, setQuantity] = useState<number>(1);
  const [reason, setReason] = useState("");

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const quantityChange = adjustmentType === "increase" ? quantity : -quantity;
    onAdjust({
      quantityChange,
      reason: reason || undefined,
    });
  };

  const newQuantity =
    adjustmentType === "increase"
      ? item.quantity + quantity
      : Math.max(0, item.quantity - quantity);

  return (
    <Dialog open={isOpen} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-md">
        <DialogHeader>
          <DialogTitle>Adjust Stock for {item.productSku}</DialogTitle>
          <DialogDescription>
            Current stock: {item.quantity} units
            <br />
            Minimum level: {item.minimumStockLevel} units
          </DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-2">
            <Label>Adjustment Type</Label>
            <Select
              value={adjustmentType}
              onValueChange={(value) =>
                setAdjustmentType(value as "increase" | "decrease")
              }
            >
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="increase">
                  <div className="flex items-center gap-2">
                    <Plus className="h-4 w-4 text-green-600" />
                    Increase Stock
                  </div>
                </SelectItem>
                <SelectItem value="decrease">
                  <div className="flex items-center gap-2">
                    <Minus className="h-4 w-4 text-red-600" />
                    Decrease Stock
                  </div>
                </SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div className="space-y-2">
            <Label htmlFor="quantity">Quantity</Label>
            <Input
              id="quantity"
              type="number"
              min="1"
              value={quantity}
              onChange={(e) => setQuantity(parseInt(e.target.value) || 1)}
              required
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="reason">Reason (Optional)</Label>
            <Textarea
              id="reason"
              placeholder="e.g., Received shipment, Damaged goods, Inventory adjustment..."
              value={reason}
              onChange={(e) => setReason(e.target.value)}
            />
          </div>

          <div className="p-4 bg-muted rounded-lg">
            <div className="text-sm text-muted-foreground">Preview:</div>
            <div className="text-lg font-semibold">
              {item.quantity} → {newQuantity} units
            </div>
            <div className="text-sm">
              {adjustmentType === "increase" ? "+" : ""}
              {adjustmentType === "increase" ? quantity : -quantity} units
            </div>
          </div>

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
            >
              Cancel
            </Button>
            <Button type="submit" disabled={isLoading}>
              {isLoading ? "Adjusting..." : "Adjust Stock"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
};

export default StockManagement;
