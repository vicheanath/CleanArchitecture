import type {
  ProductDto,
  CreateProductRequest,
  CreateProductResult,
  UpdateProductRequest,
} from "../types/types";

const API_BASE_URL = "http://localhost:5105/api";

class ProductsApi {
  private async request<T>(
    endpoint: string,
    options?: RequestInit
  ): Promise<T> {
    try {
      const response = await fetch(`${API_BASE_URL}${endpoint}`, {
        headers: {
          "Content-Type": "application/json",
          ...options?.headers,
        },
        ...options,
      });

      if (!response.ok) {
        if (response.status === 404) {
          throw new Error(`Resource not found: ${endpoint}`);
        }
        if (response.status === 400) {
          throw new Error(`Bad request: ${response.statusText}`);
        }
        if (response.status >= 500) {
          throw new Error(
            `Server error: ${response.status} ${response.statusText}`
          );
        }
        throw new Error(`API Error: ${response.status} ${response.statusText}`);
      }

      const contentType = response.headers.get("content-type");
      if (contentType && contentType.includes("application/json")) {
        return response.json();
      } else {
        return {} as T;
      }
    } catch (error) {
      if (error instanceof TypeError && error.message.includes("fetch")) {
        throw new Error(
          "Network error: Unable to connect to the server. Please check if the API is running."
        );
      }
      throw error;
    }
  }

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
    return this.request<CreateProductResult>("/products", {
      method: "POST",
      body: JSON.stringify(product),
    });
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
    return this.request<ProductDto>(`/products/${id}`, {
      method: "PUT",
      body: JSON.stringify(product),
    });
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
