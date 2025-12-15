import { ApiClient } from "@/shared/lib/apiClient";
import type {
  ProductDto,
  CreateProductRequest,
  CreateProductResult,
  UpdateProductRequest,
} from "../types/types";

class ProductsApi extends ApiClient {
  async getProducts(): Promise<ProductDto[]> {
    try {
      const products = await this.request<ProductDto[]>("/products");
      return Array.isArray(products) ? products : [];
    } catch (error) {
      console.error("Error fetching products:", error);
      throw error;
    }
  }

  async getProduct(id: string): Promise<ProductDto> {
    if (!id) {
      throw new Error("Product ID is required");
    }
    return this.request<ProductDto>(`/products/${id}`);
  }

  async createProduct(
    product: CreateProductRequest
  ): Promise<CreateProductResult> {
    if (!product || !product.sku || !product.name || !product.price) {
      throw new Error("Invalid product data: missing required fields");
    }
    const response = await this.request<{ value: CreateProductResult }>(
      "/products",
      {
        method: "POST",
        body: JSON.stringify(product),
      }
    );
    return "value" in response
      ? response.value
      : (response as CreateProductResult);
  }

  async updateProduct(
    id: string,
    product: UpdateProductRequest
  ): Promise<ProductDto> {
    if (!id) {
      throw new Error("Product ID is required");
    }
    if (!product || !product.name || !product.price) {
      throw new Error("Invalid product data: missing required fields");
    }
    const response = await this.request<{ value: ProductDto }>(
      `/products/${id}`,
      {
        method: "PUT",
        body: JSON.stringify(product),
      }
    );
    return "value" in response ? response.value : (response as ProductDto);
  }

  async deleteProduct(id: string): Promise<void> {
    if (!id) {
      throw new Error("Product ID is required");
    }
    await this.request(`/products/${id}`, {
      method: "DELETE",
    });
  }
}

export const productsApi = new ProductsApi();
