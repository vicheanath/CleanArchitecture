import React, { useState, useMemo } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import type { ColumnDef } from "@tanstack/react-table";
import {
  Package,
  AlertTriangle,
  Plus,
  Minus,
  TrendingUp,
  TrendingDown,
  RefreshCw,
  MoreHorizontal,
  Edit,
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
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import { DataTable } from "@/components/ui/data-table";
import { createSortableHeader } from "@/lib/table-utils";
import { apiService } from "@/services/api";
import type {
  LowStockItemResponse,
  AdjustInventoryStockRequest,
} from "@/types/api";

const InventoryManagementTable: React.FC = () => {
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

  // Filter inventory items by stock level
  const filteredItems = useMemo(() => {
    if (stockFilter === "all") return inventoryItems;
    return inventoryItems.filter((item) => {
      if (stockFilter === "low")
        return item.quantity < item.minimumStockLevel && item.quantity > 0;
      if (stockFilter === "out") return item.quantity === 0;
      if (stockFilter === "adequate")
        return item.quantity >= item.minimumStockLevel;
      return true;
    });
  }, [inventoryItems, stockFilter]);

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

  // Define table columns
  const columns: ColumnDef<LowStockItemResponse>[] = useMemo(
    () => [
      {
        accessorKey: "productSku",
        header: createSortableHeader<LowStockItemResponse>("Product SKU"),
        cell: ({ row }) => (
          <div className="font-mono bg-muted px-2 py-1 rounded text-sm">
            {row.getValue("productSku")}
          </div>
        ),
      },
      {
        accessorKey: "quantity",
        header: createSortableHeader<LowStockItemResponse>("Current Stock"),
        cell: ({ row }) => {
          const quantity = row.getValue("quantity") as number;
          const item = row.original;
          const isLow = quantity < item.minimumStockLevel;
          const isOut = quantity === 0;

          return (
            <div
              className={`font-bold text-lg ${
                isOut
                  ? "text-red-600"
                  : isLow
                  ? "text-yellow-600"
                  : "text-green-600"
              }`}
            >
              {quantity}
            </div>
          );
        },
      },
      {
        accessorKey: "minimumStockLevel",
        header: createSortableHeader<LowStockItemResponse>("Minimum Level"),
        cell: ({ row }) => (
          <div className="font-medium">{row.getValue("minimumStockLevel")}</div>
        ),
      },
      {
        accessorKey: "stockDeficit",
        header: createSortableHeader<LowStockItemResponse>("Stock Deficit"),
        cell: ({ row }) => {
          const deficit = row.getValue("stockDeficit") as number;
          return (
            <div
              className={`font-medium ${
                deficit > 0 ? "text-red-600" : "text-green-600"
              }`}
            >
              {deficit > 0 ? `-${deficit}` : "0"}
            </div>
          );
        },
      },
      {
        id: "status",
        header: "Status",
        cell: ({ row }) => {
          const item = row.original;
          const isOut = item.quantity === 0;
          const isLow = item.quantity < item.minimumStockLevel;

          let status = "In Stock";
          let className = "bg-green-100 text-green-800";

          if (isOut) {
            status = "Out of Stock";
            className = "bg-red-100 text-red-800";
          } else if (isLow) {
            status = "Low Stock";
            className = "bg-yellow-100 text-yellow-800";
          }

          return (
            <span
              className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${className}`}
            >
              {status}
            </span>
          );
        },
      },
      {
        id: "stockLevel",
        header: "Stock Level",
        cell: ({ row }) => {
          const item = row.original;
          const stockPercentage = Math.max(
            0,
            Math.min(100, (item.quantity / (item.minimumStockLevel * 2)) * 100)
          );

          return (
            <div className="w-full max-w-[120px]">
              <div className="flex justify-between text-xs text-muted-foreground mb-1">
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
          );
        },
      },
      {
        id: "actions",
        header: "Actions",
        cell: ({ row }) => {
          const item = row.original;

          return (
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="ghost" className="h-8 w-8 p-0">
                  <span className="sr-only">Open menu</span>
                  <MoreHorizontal className="h-4 w-4" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end">
                <DropdownMenuLabel>Actions</DropdownMenuLabel>
                <DropdownMenuItem
                  onClick={() => navigator.clipboard.writeText(item.id)}
                >
                  Copy item ID
                </DropdownMenuItem>
                <DropdownMenuSeparator />
                <DropdownMenuItem
                  onClick={() => setSelectedInventoryItem(item)}
                >
                  <Edit className="mr-2 h-4 w-4" />
                  Adjust Stock
                </DropdownMenuItem>
                <DropdownMenuItem
                  onClick={() => {
                    // TODO: Implement view details functionality
                    console.log("View details:", item.id);
                  }}
                >
                  View details
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          );
        },
      },
    ],
    [setSelectedInventoryItem]
  );

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
            Inventory Management
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

      {/* Stock Level Filter */}
      <Card>
        <CardHeader>
          <CardTitle>Filter by Stock Level</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="w-full sm:w-64">
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
        </CardContent>
      </Card>

      {/* Inventory Items Table */}
      <Card>
        <CardHeader>
          <CardTitle>Inventory Items</CardTitle>
        </CardHeader>
        <CardContent>
          <DataTable
            columns={columns}
            data={filteredItems}
            searchPlaceholder="Search by product SKU..."
            enablePagination={true}
            enableSorting={true}
            enableFiltering={true}
            pageSize={10}
          />
        </CardContent>
      </Card>

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

export default InventoryManagementTable;
