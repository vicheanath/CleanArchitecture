import { useParams, useNavigate } from "react-router-dom";
import {
  ArrowLeft,
  Edit,
  Package,
  DollarSign,
  Tag,
  Calendar,
} from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/shared/components/ui/card";
import { useProduct } from "../hooks/useProducts";

export function ProductDetailsPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const { data: product, isLoading, error } = useProduct(id!);

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="text-lg">Loading product details...</div>
      </div>
    );
  }

  if (error || !product) {
    return (
      <div className="flex flex-col items-center justify-center h-64 space-y-4">
        <div className="text-lg text-red-600">
          {error instanceof Error
            ? `Error loading product: ${error.message}`
            : "Product not found"}
        </div>
        <Button onClick={() => navigate("/products")} variant="outline">
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back to Products
        </Button>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button
            variant="ghost"
            size="icon"
            onClick={() => navigate("/products")}
          >
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <div>
            <h1 className="text-3xl font-bold tracking-tight">
              {product.name}
            </h1>
            <p className="text-muted-foreground">Product Details</p>
          </div>
        </div>
        <Button onClick={() => navigate(`/products/${id}/edit`)}>
          <Edit className="h-4 w-4 mr-2" />
          Edit Product
        </Button>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        {/* Basic Information */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Package className="h-5 w-5" />
              Basic Information
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div>
              <label className="text-sm font-medium text-muted-foreground">
                SKU
              </label>
              <p className="font-mono bg-muted px-2 py-1 rounded mt-1">
                {product.sku}
              </p>
            </div>
            <div>
              <label className="text-sm font-medium text-muted-foreground">
                Name
              </label>
              <p className="mt-1">{product.name}</p>
            </div>
            <div>
              <label className="text-sm font-medium text-muted-foreground">
                Description
              </label>
              <p className="mt-1 text-sm">{product.description}</p>
            </div>
            <div>
              <label className="text-sm font-medium text-muted-foreground">
                Category
              </label>
              <p className="mt-1">
                <span className="bg-blue-100 text-blue-800 px-2 py-1 rounded text-sm">
                  {product.category}
                </span>
              </p>
            </div>
            {product.brand && (
              <div>
                <label className="text-sm font-medium text-muted-foreground">
                  Brand
                </label>
                <p className="mt-1">{product.brand}</p>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Pricing Information */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <DollarSign className="h-5 w-5" />
              Pricing
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div>
              <label className="text-sm font-medium text-muted-foreground">
                Regular Price
              </label>
              <p className="text-2xl font-bold mt-1">
                ${product.price.toFixed(2)}
              </p>
            </div>
            {product.isOnSale && (
              <>
                <div>
                  <label className="text-sm font-medium text-muted-foreground">
                    Sale Price
                  </label>
                  <p className="text-xl font-bold text-green-600 mt-1">
                    ${product.salePrice?.toFixed(2)}
                  </p>
                </div>
                {product.saleStartDate && product.saleEndDate && (
                  <div>
                    <label className="text-sm font-medium text-muted-foreground">
                      Sale Period
                    </label>
                    <p className="text-sm mt-1">
                      {new Date(product.saleStartDate).toLocaleDateString()} -{" "}
                      {new Date(product.saleEndDate).toLocaleDateString()}
                    </p>
                  </div>
                )}
              </>
            )}
            <div>
              <label className="text-sm font-medium text-muted-foreground">
                Effective Price
              </label>
              <p className="text-2xl font-bold text-green-600 mt-1">
                ${product.effectivePrice.toFixed(2)}
              </p>
            </div>
          </CardContent>
        </Card>

        {/* Physical Attributes */}
        {(product.weight > 0 ||
          product.dimensions ||
          product.color ||
          product.size) && (
          <Card>
            <CardHeader>
              <CardTitle>Physical Attributes</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              {product.weight > 0 && (
                <div>
                  <label className="text-sm font-medium text-muted-foreground">
                    Weight
                  </label>
                  <p className="mt-1">{product.weight} lbs</p>
                </div>
              )}
              {product.dimensions && (
                <div>
                  <label className="text-sm font-medium text-muted-foreground">
                    Dimensions
                  </label>
                  <p className="mt-1">{product.dimensions}</p>
                </div>
              )}
              {product.color && (
                <div>
                  <label className="text-sm font-medium text-muted-foreground">
                    Color
                  </label>
                  <p className="mt-1">{product.color}</p>
                </div>
              )}
              {product.size && (
                <div>
                  <label className="text-sm font-medium text-muted-foreground">
                    Size
                  </label>
                  <p className="mt-1">{product.size}</p>
                </div>
              )}
            </CardContent>
          </Card>
        )}

        {/* Additional Information */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Tag className="h-5 w-5" />
              Additional Information
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div>
              <label className="text-sm font-medium text-muted-foreground">
                Status
              </label>
              <p className="mt-1">
                <span
                  className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                    product.isActive
                      ? "bg-green-100 text-green-800"
                      : "bg-red-100 text-red-800"
                  }`}
                >
                  {product.isActive ? "Active" : "Inactive"}
                </span>
              </p>
            </div>
            <div>
              <label className="text-sm font-medium text-muted-foreground">
                Featured
              </label>
              <p className="mt-1">{product.isFeatured ? "Yes" : "No"}</p>
            </div>
            <div>
              <label className="text-sm font-medium text-muted-foreground">
                Requires Shipping
              </label>
              <p className="mt-1">{product.requiresShipping ? "Yes" : "No"}</p>
            </div>
            {product.requiresShipping && product.shippingWeight > 0 && (
              <div>
                <label className="text-sm font-medium text-muted-foreground">
                  Shipping Weight
                </label>
                <p className="mt-1">{product.shippingWeight} lbs</p>
              </div>
            )}
            <div>
              <label className="text-sm font-medium text-muted-foreground flex items-center gap-2">
                <Calendar className="h-4 w-4" />
                Created On
              </label>
              <p className="mt-1 text-sm">
                {new Date(product.createdOnUtc).toLocaleString()}
              </p>
            </div>
          </CardContent>
        </Card>

        {/* Images */}
        {product.images && product.images.length > 0 && (
          <Card>
            <CardHeader>
              <CardTitle>Images</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-2 gap-4">
                {product.images.map((imageUrl, index) => (
                  <img
                    key={index}
                    src={imageUrl}
                    alt={`${product.name} - Image ${index + 1}`}
                    className="rounded-lg object-cover w-full h-48"
                    onError={(e) => {
                      (e.target as HTMLImageElement).style.display = "none";
                    }}
                  />
                ))}
              </div>
            </CardContent>
          </Card>
        )}

        {/* Tags */}
        {product.tags && product.tags.length > 0 && (
          <Card>
            <CardHeader>
              <CardTitle>Tags</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="flex flex-wrap gap-2">
                {product.tags.map((tag, index) => (
                  <span
                    key={index}
                    className="bg-muted px-2 py-1 rounded text-sm"
                  >
                    {tag}
                  </span>
                ))}
              </div>
            </CardContent>
          </Card>
        )}
      </div>
    </div>
  );
}
