// Product types
export interface ProductDto {
  id: string;
  sku: string;
  name: string;
  description: string;
  price: number;
  category: string;
  brand: string;
  weight: number;
  dimensions: string;
  color: string;
  size: string;
  salePrice?: number;
  saleStartDate?: string;
  saleEndDate?: string;
  metaTitle: string;
  metaDescription: string;
  requiresShipping: boolean;
  shippingWeight: number;
  isFeatured: boolean;
  sortOrder: number;
  images: string[];
  tags: string[];
  effectivePrice: number;
  isOnSale: boolean;
  isActive: boolean;
  createdOnUtc: string;
}

export interface CreateProductRequest {
  sku: string;
  name: string;
  description: string;
  price: number;
  category: string;
  brand?: string;
  weight?: number;
  dimensions?: string;
  color?: string;
  size?: string;
  salePrice?: number;
  saleStartDate?: string;
  saleEndDate?: string;
  metaTitle?: string;
  metaDescription?: string;
  requiresShipping?: boolean;
  shippingWeight?: number;
  isFeatured?: boolean;
  sortOrder?: number;
  images?: string[];
  tags?: string[];
}

export interface CreateProductResult {
  id: string;
  sku: string;
  name: string;
  description: string;
  price: number;
  category: string;
  brand: string;
  weight: number;
  dimensions: string;
  color: string;
  size: string;
  salePrice?: number;
  saleStartDate?: string;
  saleEndDate?: string;
  metaTitle: string;
  metaDescription: string;
  requiresShipping: boolean;
  shippingWeight: number;
  isFeatured: boolean;
  sortOrder: number;
  images: string[];
  tags: string[];
  createdOnUtc: string;
}

export interface UpdateProductRequest {
  name: string;
  description: string;
  price: number;
  category: string;
  brand?: string;
  weight?: number;
  dimensions?: string;
  color?: string;
  size?: string;
  salePrice?: number;
  saleStartDate?: string;
  saleEndDate?: string;
  metaTitle?: string;
  metaDescription?: string;
  requiresShipping?: boolean;
  shippingWeight?: number;
  isFeatured?: boolean;
  sortOrder?: number;
  images?: string[];
  tags?: string[];
}
