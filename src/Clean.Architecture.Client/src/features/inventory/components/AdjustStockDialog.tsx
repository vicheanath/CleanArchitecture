import React, { useState } from "react";
import { Plus, Minus } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/ui/dialog";
import { Input } from "@/shared/components/ui/input";
import { Label } from "@/shared/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/ui/select";
import { Textarea } from "@/shared/components/ui/textarea";
import type { LowStockItemResponse, AdjustInventoryStockRequest } from "../types/types";

interface AdjustStockDialogProps {
  item: LowStockItemResponse;
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
  onAdjust: (adjustment: AdjustInventoryStockRequest) => void;
  isLoading: boolean;
}

export const AdjustStockDialog: React.FC<AdjustStockDialogProps> = ({
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
