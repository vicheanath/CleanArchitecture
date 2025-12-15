import { Edit, Calendar } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/shared/components/ui/card";
import { PermissionGate } from "@/shared/components/PermissionGate";
import { PERMISSIONS } from "@/shared/constants/permissions";
import type { ProductDto } from "../types/types";

interface ProductCardProps {
  product: ProductDto;
  onEdit: (product: ProductDto) => void;
}

export const ProductCard: React.FC<ProductCardProps> = ({
  product,
  onEdit,
}) => {
  return (
    <Card className="group hover:shadow-lg transition-shadow">
      <CardHeader>
        <div className="flex justify-between items-start">
          <div className="space-y-1">
            <CardTitle className="text-lg line-clamp-1">
              {product.name}
            </CardTitle>
            <div className="flex items-center gap-2 text-sm text-muted-foreground">
              <span className="font-mono bg-muted px-2 py-1 rounded">
                {product.sku}
              </span>
              <span className="bg-blue-100 text-blue-800 px-2 py-1 rounded">
                {product.category}
              </span>
            </div>
          </div>
          <PermissionGate permission={PERMISSIONS.PRODUCTS_WRITE}>
            <div className="flex gap-1">
              <Button
                variant="ghost"
                size="icon"
                onClick={() => onEdit(product)}
                className="opacity-0 group-hover:opacity-100 transition-opacity"
              >
                <Edit className="h-4 w-4" />
              </Button>
            </div>
          </PermissionGate>
        </div>
      </CardHeader>
      <CardContent className="space-y-4">
        <p className="text-sm text-muted-foreground line-clamp-2">
          {product.description}
        </p>
        <div className="flex justify-between items-center">
          <div className="text-2xl font-bold text-green-600">
            ${product.price.toFixed(2)}
          </div>
          <div className="flex items-center gap-2 text-sm text-muted-foreground">
            <Calendar className="h-4 w-4" />
            {new Date(product.createdOnUtc).toLocaleDateString()}
          </div>
        </div>
        <div className="flex items-center justify-between">
          <span
            className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
              product.isActive
                ? "bg-green-100 text-green-800"
                : "bg-red-100 text-red-800"
            }`}
          >
            {product.isActive ? "Active" : "Inactive"}
          </span>
        </div>
      </CardContent>
    </Card>
  );
};
