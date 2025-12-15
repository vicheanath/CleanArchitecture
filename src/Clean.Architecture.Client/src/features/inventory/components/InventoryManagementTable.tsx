import React, { useState, useMemo } from "react";
import type { ColumnDef } from "@tanstack/react-table";
import {
  Package,
  AlertTriangle,
  TrendingUp,
  TrendingDown,
  RefreshCw,
  MoreHorizontal,
  Edit,
} from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/shared/components/ui/card";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/ui/select";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/shared/components/ui/dropdown-menu";
import { DataTable } from "@/shared/components/ui/data-table";
import { createSortableHeader } from "@/shared/lib/table-utils";
import { useLowStockItems } from "../hooks/useInventory";
import { useAdjustInventoryStock } from "../hooks/useAdjustInventoryStock";
import { AdjustStockDialog } from "./AdjustStockDialog";
import type { LowStockItemResponse } from "../types/types";

export const InventoryManagementTable: React.FC = () => {
  const [stockFilter, setStockFilter] = useState<string>("all");
  const [selectedInventoryItem, setSelectedInventoryItem] =
    useState<LowStockItemResponse | null>(null);

  const {
    data: inventoryItems = [],
    isLoading,
    error,
    refetch,
  } = useLowStockItems();
  const adjustStockMutation = useAdjustInventoryStock();

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
              </DropdownMenuContent>
            </DropdownMenu>
          );
        },
      },
    ],
    []
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
            adjustStockMutation.mutate(
              {
                id: selectedInventoryItem.id,
                adjustment,
              },
              {
                onSuccess: () => {
                  setSelectedInventoryItem(null);
                },
              }
            );
          }}
          isLoading={adjustStockMutation.isPending}
        />
      )}
    </div>
  );
};
