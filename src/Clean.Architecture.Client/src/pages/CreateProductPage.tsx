import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import {
  ArrowLeft,
  Plus,
  X,
  Package,
  DollarSign,
  Truck,
  Tag,
  Image,
  Star,
  Search,
  Palette,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Checkbox } from "@/components/ui/checkbox";
import { Separator } from "@/components/ui/separator";
import { Badge } from "@/components/ui/badge";
import { apiService } from "@/services/api";
import { z } from "zod";
import type { CreateProductRequest } from "@/types/api";

// Form validation schema
const createProductFormSchema = z
  .object({
    sku: z
      .string()
      .min(1, "SKU is required")
      .max(50, "SKU must be less than 50 characters")
      .regex(
        /^[A-Z0-9-_]+$/,
        "SKU must contain only uppercase letters, numbers, hyphens, and underscores"
      ),

    name: z
      .string()
      .min(1, "Product name is required")
      .max(200, "Product name must be less than 200 characters"),

    description: z
      .string()
      .min(1, "Description is required")
      .max(2000, "Description must be less than 2000 characters"),

    price: z
      .number()
      .min(0.01, "Price must be greater than 0")
      .max(999999.99, "Price must be less than 1,000,000"),

    category: z
      .string()
      .min(1, "Category is required")
      .max(100, "Category must be less than 100 characters"),

    brand: z
      .string()
      .max(100, "Brand must be less than 100 characters")
      .optional(),

    weight: z
      .number()
      .min(0, "Weight cannot be negative")
      .max(9999.99, "Weight must be less than 10,000")
      .optional(),

    dimensions: z
      .string()
      .max(100, "Dimensions must be less than 100 characters")
      .optional(),

    color: z
      .string()
      .max(50, "Color must be less than 50 characters")
      .optional(),

    size: z.string().max(50, "Size must be less than 50 characters").optional(),

    salePrice: z
      .number()
      .min(0.01, "Sale price must be greater than 0")
      .max(999999.99, "Sale price must be less than 1,000,000")
      .optional()
      .nullable(),

    saleStartDate: z.string().optional(),

    saleEndDate: z.string().optional(),

    metaTitle: z
      .string()
      .max(200, "Meta title must be less than 200 characters")
      .optional(),

    metaDescription: z
      .string()
      .max(500, "Meta description must be less than 500 characters")
      .optional(),

    requiresShipping: z.boolean().optional(),

    shippingWeight: z
      .number()
      .min(0, "Shipping weight cannot be negative")
      .max(9999.99, "Shipping weight must be less than 10,000")
      .optional(),

    isFeatured: z.boolean().optional(),

    sortOrder: z
      .number()
      .int("Sort order must be an integer")
      .min(0, "Sort order cannot be negative")
      .max(999999, "Sort order must be less than 1,000,000")
      .optional(),

    images: z.array(z.string().url("Invalid image URL")).optional(),

    tags: z.array(z.string().min(1, "Tag cannot be empty")).optional(),
  })
  .superRefine((data, ctx) => {
    // Validate sale dates
    if (data.saleStartDate && data.saleEndDate) {
      const startDate = new Date(data.saleStartDate);
      const endDate = new Date(data.saleEndDate);

      if (endDate <= startDate) {
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          message: "Sale end date must be after sale start date",
          path: ["saleEndDate"],
        });
      }
    }

    // Validate sale price
    if (data.salePrice && data.salePrice >= data.price) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        message: "Sale price must be less than regular price",
        path: ["salePrice"],
      });
    }
  });

type CreateProductFormData = z.infer<typeof createProductFormSchema>;

export function CreateProductPage() {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [newTag, setNewTag] = useState("");
  const [newImage, setNewImage] = useState("");

  const {
    register,
    handleSubmit,
    control,
    setValue,
    watch,
    formState: { errors },
  } = useForm<CreateProductFormData>({
    resolver: zodResolver(createProductFormSchema),
    defaultValues: {
      sku: "",
      name: "",
      description: "",
      price: 0,
      category: "",
      brand: "",
      weight: 0,
      dimensions: "",
      color: "",
      size: "",
      salePrice: undefined,
      saleStartDate: "",
      saleEndDate: "",
      metaTitle: "",
      metaDescription: "",
      requiresShipping: true,
      shippingWeight: 0,
      isFeatured: false,
      sortOrder: 0,
      images: [],
      tags: [],
    },
  });

  const watchedTags = watch("tags") || [];
  const watchedImages = watch("images") || [];

  const onSubmit = async (data: CreateProductFormData) => {
    try {
      setLoading(true);

      // Transform form data to API request format
      const apiData: CreateProductRequest = {
        sku: data.sku,
        name: data.name,
        description: data.description,
        price: data.price,
        category: data.category,
        brand: data.brand || undefined,
        weight: data.weight || undefined,
        dimensions: data.dimensions || undefined,
        color: data.color || undefined,
        size: data.size || undefined,
        salePrice: data.salePrice || undefined,
        saleStartDate: data.saleStartDate || undefined,
        saleEndDate: data.saleEndDate || undefined,
        metaTitle: data.metaTitle || undefined,
        metaDescription: data.metaDescription || undefined,
        requiresShipping: data.requiresShipping,
        shippingWeight: data.shippingWeight || undefined,
        isFeatured: data.isFeatured,
        sortOrder: data.sortOrder || undefined,
        images: data.images || [],
        tags: data.tags || [],
      };

      await apiService.createProduct(apiData);
      navigate("/products");
    } catch (error) {
      console.error("Failed to create product:", error);
      alert("Failed to create product. Please try again.");
    } finally {
      setLoading(false);
    }
  };

  const addTag = () => {
    if (newTag.trim() && !watchedTags.includes(newTag.trim())) {
      setValue("tags", [...watchedTags, newTag.trim()]);
      setNewTag("");
    }
  };

  const removeTag = (tagToRemove: string) => {
    setValue(
      "tags",
      watchedTags.filter((tag) => tag !== tagToRemove)
    );
  };

  const addImage = () => {
    if (newImage.trim() && !watchedImages.includes(newImage.trim())) {
      setValue("images", [...watchedImages, newImage.trim()]);
      setNewImage("");
    }
  };

  const removeImage = (imageToRemove: string) => {
    setValue(
      "images",
      watchedImages.filter((image) => image !== imageToRemove)
    );
  };

  const popularCategories = [
    "Electronics",
    "Clothing",
    "Books",
    "Home & Garden",
    "Sports",
    "Toys",
    "Beauty",
    "Automotive",
    "Health",
    "Food",
  ];

  const popularBrands = [
    "Apple",
    "Samsung",
    "Nike",
    "Adidas",
    "Sony",
    "Microsoft",
    "Canon",
    "LG",
    "HP",
    "Dell",
  ];

  const sizeOptions = ["XS", "S", "M", "L", "XL", "XXL", "XXXL", "One Size"];

  return (
    <div className="space-y-6 max-w-6xl mx-auto">
      {/* Header */}
      <div className="flex items-center space-x-4">
        <Button
          variant="outline"
          size="sm"
          onClick={() => navigate("/products")}
        >
          <ArrowLeft className="mr-2 h-4 w-4" />
          Back to Products
        </Button>
        <h1 className="text-3xl font-bold tracking-tight">Create Product</h1>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Basic Information */}
          <Card className="lg:col-span-2">
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Package className="h-5 w-5" />
                Basic Information
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="sku">SKU *</Label>
                  <Input
                    id="sku"
                    {...register("sku")}
                    placeholder="e.g., LAPTOP-001"
                    className={`font-mono ${
                      errors.sku ? "border-red-500" : ""
                    }`}
                  />
                  {errors.sku && (
                    <p className="text-sm text-red-500">{errors.sku.message}</p>
                  )}
                </div>

                <div className="space-y-2">
                  <Label htmlFor="category">Category *</Label>
                  <Input
                    id="category"
                    {...register("category")}
                    placeholder="Select or type category"
                    className={errors.category ? "border-red-500" : ""}
                    list="categories"
                  />
                  <datalist id="categories">
                    {popularCategories.map((cat) => (
                      <option key={cat} value={cat} />
                    ))}
                  </datalist>
                  {errors.category && (
                    <p className="text-sm text-red-500">
                      {errors.category.message}
                    </p>
                  )}
                </div>
              </div>

              <div className="space-y-2">
                <Label htmlFor="name">Product Name *</Label>
                <Input
                  id="name"
                  {...register("name")}
                  placeholder="e.g., MacBook Pro 16-inch"
                  className={errors.name ? "border-red-500" : ""}
                />
                {errors.name && (
                  <p className="text-sm text-red-500">{errors.name.message}</p>
                )}
              </div>

              <div className="space-y-2">
                <Label htmlFor="description">Description *</Label>
                <Textarea
                  id="description"
                  {...register("description")}
                  placeholder="Detailed product description..."
                  rows={4}
                  className={errors.description ? "border-red-500" : ""}
                />
                {errors.description && (
                  <p className="text-sm text-red-500">
                    {errors.description.message}
                  </p>
                )}
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="brand">Brand</Label>
                  <Input
                    id="brand"
                    {...register("brand")}
                    placeholder="Product brand"
                    className={errors.brand ? "border-red-500" : ""}
                    list="brands"
                  />
                  <datalist id="brands">
                    {popularBrands.map((brand) => (
                      <option key={brand} value={brand} />
                    ))}
                  </datalist>
                  {errors.brand && (
                    <p className="text-sm text-red-500">
                      {errors.brand.message}
                    </p>
                  )}
                </div>

                <div className="space-y-2">
                  <Label htmlFor="price">Price * ($)</Label>
                  <Input
                    id="price"
                    type="number"
                    step="0.01"
                    {...register("price", { valueAsNumber: true })}
                    placeholder="0.00"
                    className={errors.price ? "border-red-500" : ""}
                  />
                  {errors.price && (
                    <p className="text-sm text-red-500">
                      {errors.price.message}
                    </p>
                  )}
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Product Attributes */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Palette className="h-5 w-5" />
                Attributes
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="color">Color</Label>
                <Input
                  id="color"
                  {...register("color")}
                  placeholder="e.g., Space Gray"
                  className={errors.color ? "border-red-500" : ""}
                />
                {errors.color && (
                  <p className="text-sm text-red-500">{errors.color.message}</p>
                )}
              </div>

              <div className="space-y-2">
                <Label htmlFor="size">Size</Label>
                <Input
                  id="size"
                  {...register("size")}
                  placeholder="Select or type size"
                  className={errors.size ? "border-red-500" : ""}
                  list="sizes"
                />
                <datalist id="sizes">
                  {sizeOptions.map((size) => (
                    <option key={size} value={size} />
                  ))}
                </datalist>
                {errors.size && (
                  <p className="text-sm text-red-500">{errors.size.message}</p>
                )}
              </div>

              <div className="space-y-2">
                <Label htmlFor="dimensions">Dimensions</Label>
                <Input
                  id="dimensions"
                  {...register("dimensions")}
                  placeholder="L x W x H (inches)"
                  className={errors.dimensions ? "border-red-500" : ""}
                />
                {errors.dimensions && (
                  <p className="text-sm text-red-500">
                    {errors.dimensions.message}
                  </p>
                )}
              </div>

              <div className="space-y-2">
                <Label htmlFor="weight">Weight (lbs)</Label>
                <Input
                  id="weight"
                  type="number"
                  step="0.01"
                  {...register("weight", { valueAsNumber: true })}
                  placeholder="0.00"
                  className={errors.weight ? "border-red-500" : ""}
                />
                {errors.weight && (
                  <p className="text-sm text-red-500">
                    {errors.weight.message}
                  </p>
                )}
              </div>
            </CardContent>
          </Card>

          {/* Pricing & Sales */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <DollarSign className="h-5 w-5" />
                Pricing & Sales
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="salePrice">Sale Price ($)</Label>
                <Input
                  id="salePrice"
                  type="number"
                  step="0.01"
                  {...register("salePrice", { valueAsNumber: true })}
                  placeholder="Optional sale price"
                  className={errors.salePrice ? "border-red-500" : ""}
                />
                {errors.salePrice && (
                  <p className="text-sm text-red-500">
                    {errors.salePrice.message}
                  </p>
                )}
              </div>

              <div className="space-y-2">
                <Label htmlFor="saleStartDate">Sale Start Date</Label>
                <Input
                  id="saleStartDate"
                  type="datetime-local"
                  {...register("saleStartDate")}
                  className={errors.saleStartDate ? "border-red-500" : ""}
                />
                {errors.saleStartDate && (
                  <p className="text-sm text-red-500">
                    {errors.saleStartDate.message}
                  </p>
                )}
              </div>

              <div className="space-y-2">
                <Label htmlFor="saleEndDate">Sale End Date</Label>
                <Input
                  id="saleEndDate"
                  type="datetime-local"
                  {...register("saleEndDate")}
                  className={errors.saleEndDate ? "border-red-500" : ""}
                />
                {errors.saleEndDate && (
                  <p className="text-sm text-red-500">
                    {errors.saleEndDate.message}
                  </p>
                )}
              </div>

              <div className="flex items-center space-x-2">
                <Controller
                  name="isFeatured"
                  control={control}
                  render={({ field }) => (
                    <Checkbox
                      id="isFeatured"
                      checked={field.value}
                      onCheckedChange={field.onChange}
                    />
                  )}
                />
                <Label htmlFor="isFeatured" className="flex items-center gap-2">
                  <Star className="h-4 w-4" />
                  Featured Product
                </Label>
              </div>

              <div className="space-y-2">
                <Label htmlFor="sortOrder">Sort Order</Label>
                <Input
                  id="sortOrder"
                  type="number"
                  {...register("sortOrder", { valueAsNumber: true })}
                  placeholder="0"
                  className={errors.sortOrder ? "border-red-500" : ""}
                />
                {errors.sortOrder && (
                  <p className="text-sm text-red-500">
                    {errors.sortOrder.message}
                  </p>
                )}
              </div>
            </CardContent>
          </Card>

          {/* Shipping Information */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Truck className="h-5 w-5" />
                Shipping
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="flex items-center space-x-2">
                <Controller
                  name="requiresShipping"
                  control={control}
                  render={({ field }) => (
                    <Checkbox
                      id="requiresShipping"
                      checked={field.value}
                      onCheckedChange={field.onChange}
                    />
                  )}
                />
                <Label htmlFor="requiresShipping">Requires Shipping</Label>
              </div>

              <div className="space-y-2">
                <Label htmlFor="shippingWeight">Shipping Weight (lbs)</Label>
                <Input
                  id="shippingWeight"
                  type="number"
                  step="0.01"
                  {...register("shippingWeight", { valueAsNumber: true })}
                  placeholder="0.00"
                  className={errors.shippingWeight ? "border-red-500" : ""}
                />
                {errors.shippingWeight && (
                  <p className="text-sm text-red-500">
                    {errors.shippingWeight.message}
                  </p>
                )}
              </div>
            </CardContent>
          </Card>

          {/* SEO Information */}
          <Card className="lg:col-span-2">
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Search className="h-5 w-5" />
                SEO Information
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="metaTitle">Meta Title</Label>
                <Input
                  id="metaTitle"
                  {...register("metaTitle")}
                  placeholder="SEO title (auto-generated from product name if empty)"
                  maxLength={60}
                  className={errors.metaTitle ? "border-red-500" : ""}
                />
                {errors.metaTitle && (
                  <p className="text-sm text-red-500">
                    {errors.metaTitle.message}
                  </p>
                )}
                <p className="text-xs text-muted-foreground">
                  {watch("metaTitle")?.length || 0}/60 characters
                </p>
              </div>

              <div className="space-y-2">
                <Label htmlFor="metaDescription">Meta Description</Label>
                <Textarea
                  id="metaDescription"
                  {...register("metaDescription")}
                  placeholder="SEO description (auto-generated from description if empty)"
                  maxLength={160}
                  rows={3}
                  className={errors.metaDescription ? "border-red-500" : ""}
                />
                {errors.metaDescription && (
                  <p className="text-sm text-red-500">
                    {errors.metaDescription.message}
                  </p>
                )}
                <p className="text-xs text-muted-foreground">
                  {watch("metaDescription")?.length || 0}/160 characters
                </p>
              </div>
            </CardContent>
          </Card>

          {/* Images */}
          <Card className="lg:col-span-3">
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Image className="h-5 w-5" />
                Product Images
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="flex space-x-2">
                <Input
                  type="url"
                  value={newImage}
                  onChange={(e) => setNewImage(e.target.value)}
                  placeholder="Image URL (e.g., https://example.com/image.jpg)"
                  className="flex-1"
                />
                <Button
                  type="button"
                  onClick={addImage}
                  disabled={!newImage.trim()}
                  variant="outline"
                >
                  <Plus className="h-4 w-4" />
                </Button>
              </div>

              {watchedImages.length > 0 && (
                <div className="space-y-2">
                  <Label>Current Images:</Label>
                  <div className="space-y-2">
                    {watchedImages.map((image, index) => (
                      <div
                        key={index}
                        className="flex items-center justify-between p-2 border rounded"
                      >
                        <span className="text-sm font-mono truncate flex-1">
                          {image}
                        </span>
                        <Button
                          type="button"
                          variant="outline"
                          size="sm"
                          onClick={() => removeImage(image)}
                        >
                          <X className="h-4 w-4" />
                        </Button>
                      </div>
                    ))}
                  </div>
                </div>
              )}

              {errors.images && (
                <p className="text-sm text-red-500">{errors.images.message}</p>
              )}
            </CardContent>
          </Card>

          {/* Tags */}
          <Card className="lg:col-span-3">
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Tag className="h-5 w-5" />
                Product Tags
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="flex space-x-2">
                <Input
                  type="text"
                  value={newTag}
                  onChange={(e) => setNewTag(e.target.value)}
                  placeholder="Add a tag (e.g., gaming, portable, premium)"
                  className="flex-1"
                  onKeyPress={(e) =>
                    e.key === "Enter" && (e.preventDefault(), addTag())
                  }
                />
                <Button
                  type="button"
                  onClick={addTag}
                  disabled={!newTag.trim()}
                  variant="outline"
                >
                  <Plus className="h-4 w-4" />
                </Button>
              </div>

              {watchedTags.length > 0 && (
                <div className="space-y-2">
                  <Label>Current Tags:</Label>
                  <div className="flex flex-wrap gap-2">
                    {watchedTags.map((tag, index) => (
                      <Badge
                        key={index}
                        variant="secondary"
                        className="flex items-center gap-1"
                      >
                        {tag}
                        <Button
                          type="button"
                          variant="ghost"
                          size="sm"
                          className="h-auto p-0 hover:bg-transparent"
                          onClick={() => removeTag(tag)}
                        >
                          <X className="h-3 w-3" />
                        </Button>
                      </Badge>
                    ))}
                  </div>
                </div>
              )}

              {errors.tags && (
                <p className="text-sm text-red-500">{errors.tags.message}</p>
              )}
            </CardContent>
          </Card>
        </div>

        <Separator />

        <div className="flex justify-end space-x-2 pt-4">
          <Button
            type="button"
            variant="outline"
            onClick={() => navigate("/products")}
            disabled={loading}
          >
            Cancel
          </Button>
          <Button type="submit" disabled={loading}>
            {loading ? "Creating Product..." : "Create Product"}
          </Button>
        </div>
      </form>
    </div>
  );
}
