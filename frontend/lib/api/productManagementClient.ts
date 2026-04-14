import { Product } from "../types";

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5047/api";

export interface CreateProductRequest {
  sku: string;
  name: string;
  description?: string;
  brand?: string;
  category: string;
  basePrice: number;
  availableQuantity: number;
}

export interface UpdateProductRequest extends CreateProductRequest {
  isActive?: boolean;
  priority?: number;
}

export interface ProductListResponse {
  productId: number;
  sku: string;
  name: string;
  description?: string;
  brand?: string;
  category: string;
  basePrice: number;
  availableQuantity: number;
  soldQuantity: number;
  priority: number;
  hasPrescription: boolean;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

class ProductManagementClient {
  private getHeaders() {
    const token = localStorage.getItem("authToken");
    const headers: Record<string, string> = {
      "Content-Type": "application/json",
    };
    
    if (token) {
      headers.Authorization = `Bearer ${token}`;
    }
    
    return headers;
  }

  async createProduct(data: CreateProductRequest): Promise<Product> {
    console.log("🔨 Creating product...", data);
    const response = await fetch(`${API_URL}/products`, {
      method: "POST",
      headers: this.getHeaders(),
      body: JSON.stringify({
        sku: data.sku,
        name: data.name,
        description: data.description || null,
        brand: data.brand || null,
        category: data.category,
        basePrice: data.basePrice,
        availableQuantity: data.availableQuantity,
      }),
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || "Failed to create product");
    }

    const product: Product = await response.json();
    console.log("✅ Product created successfully:", product);
    return product;
  }

  async updateProduct(
    id: number,
    data: UpdateProductRequest
  ): Promise<Product> {
    console.log("🔄 Updating product...", id, data);
    const response = await fetch(`${API_URL}/products/${id}`, {
      method: "PUT",
      headers: this.getHeaders(),
      body: JSON.stringify({
        sku: data.sku,
        name: data.name,
        description: data.description || null,
        brand: data.brand || null,
        category: data.category,
        basePrice: data.basePrice,
        availableQuantity: data.availableQuantity,
        isActive: data.isActive !== undefined ? data.isActive : true,
        priority: data.priority !== undefined ? data.priority : 0,
      }),
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || "Failed to update product");
    }

    const product: Product = await response.json();
    console.log("✅ Product updated successfully:", product);
    return product;
  }

  async getAllProducts(): Promise<ProductListResponse[]> {
    console.log("📦 Fetching all products...");
    const response = await fetch(`${API_URL}/products`, {
      headers: this.getHeaders(),
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || "Failed to fetch products");
    }

    const products: ProductListResponse[] = await response.json();
    console.log("✅ Products fetched successfully:", products.length);
    return products;
  }

  async getProductById(id: number): Promise<Product> {
    console.log("🔍 Fetching product by ID...", id);
    const response = await fetch(`${API_URL}/products/${id}`, {
      headers: this.getHeaders(),
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || "Failed to fetch product");
    }

    const product: Product = await response.json();
    console.log("✅ Product fetched successfully:", product);
    return product;
  }

  async deleteProduct(id: number): Promise<void> {
    console.log("🗑️ Deleting product...", id);
    const response = await fetch(`${API_URL}/products/${id}`, {
      method: "DELETE",
      headers: this.getHeaders(),
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || "Failed to delete product");
    }

    console.log("✅ Product deleted successfully:", id);
  }
}

export const ProductManagementClientInstance = new ProductManagementClient();
