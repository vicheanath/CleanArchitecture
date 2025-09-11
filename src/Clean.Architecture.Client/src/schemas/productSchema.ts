import { z } from "zod";

const baseProductSchema = z.object({
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
    .optional()
    .default(""),

  weight: z
    .number()
    .min(0, "Weight cannot be negative")
    .max(9999.99, "Weight must be less than 10,000")
    .optional()
    .default(0),

  dimensions: z
    .string()
    .max(50, "Dimensions must be less than 50 characters")
    .optional()
    .default(""),

  color: z
    .string()
    .max(50, "Color must be less than 50 characters")
    .optional()
    .default(""),

  size: z
    .string()
    .max(50, "Size must be less than 50 characters")
    .optional()
    .default(""),

  salePrice: z
    .number()
    .min(0.01, "Sale price must be greater than 0")
    .max(999999.99, "Sale price must be less than 1,000,000")
    .optional()
    .nullable(),

  saleStartDate: z
    .string()
    .datetime()
    .optional()
    .nullable()
    .transform((val: string | null | undefined) =>
      val ? new Date(val) : null
    ),

  saleEndDate: z
    .string()
    .datetime()
    .optional()
    .nullable()
    .transform((val: string | null | undefined) =>
      val ? new Date(val) : null
    ),

  metaTitle: z
    .string()
    .max(200, "Meta title must be less than 200 characters")
    .optional()
    .default(""),

  metaDescription: z
    .string()
    .max(500, "Meta description must be less than 500 characters")
    .optional()
    .default(""),

  requiresShipping: z.boolean().optional().default(true),

  shippingWeight: z
    .number()
    .min(0, "Shipping weight cannot be negative")
    .max(9999.99, "Shipping weight must be less than 10,000")
    .optional()
    .default(0),

  isFeatured: z.boolean().optional().default(false),

  sortOrder: z
    .number()
    .int("Sort order must be an integer")
    .min(0, "Sort order cannot be negative")
    .max(999999, "Sort order must be less than 1,000,000")
    .optional()
    .default(0),

  images: z
    .array(z.string().url("Invalid image URL"))
    .max(10, "Maximum 10 images allowed")
    .optional()
    .default([]),

  tags: z
    .array(
      z
        .string()
        .min(1, "Tag cannot be empty")
        .max(50, "Tag must be less than 50 characters")
    )
    .max(20, "Maximum 20 tags allowed")
    .optional()
    .default([]),
});

export const createProductSchema = baseProductSchema.superRefine(
  (data, ctx) => {
    // If sale price is provided, it must be less than regular price
    if (data.salePrice && data.salePrice >= data.price) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        message: "Sale price must be less than regular price",
        path: ["salePrice"],
      });
    }

    // If both sale dates are provided, start date must be before end date
    if (
      data.saleStartDate &&
      data.saleEndDate &&
      data.saleStartDate >= data.saleEndDate
    ) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        message: "Sale start date must be before end date",
        path: ["saleStartDate"],
      });
    }
  }
);

export type CreateProductFormData = z.infer<typeof createProductSchema>;

// Helper function to convert form data to API request format
export function transformFormDataToApiRequest(data: CreateProductFormData) {
  return {
    ...data,
    saleStartDate: data.saleStartDate || undefined,
    saleEndDate: data.saleEndDate || undefined,
    salePrice: data.salePrice || undefined,
  };
}
