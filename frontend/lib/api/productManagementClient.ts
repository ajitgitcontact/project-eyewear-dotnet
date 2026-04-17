import { Product } from "../types";

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5047/api";

export interface CreateProductImageRequest {
  imageUrl: string;
  isPrimary: boolean;
  displayOrder: number;
}

export interface CreateCustomizationImageRequest {
  imageUrl: string;
}

export interface CreateCustomizationValueRequest {
  value: string;
  additionalPrice: number;
  customizationImages: CreateCustomizationImageRequest[];
}

export interface CreateCustomizationOptionRequest {
  name: string;
  isRequired: boolean;
  displayOrder: number;
  values: CreateCustomizationValueRequest[];
}

export interface CreateFullProductRequest {
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
  images: CreateProductImageRequest[];
  customizationOptions: CreateCustomizationOptionRequest[];
}

export interface UpdateProductRequest {
  sku: string;
  name: string;
  description?: string;
  brand?: string;
  category: string;
  basePrice: number;
  availableQuantity: number;
  isActive?: boolean;
  priority?: number;
}

export type ProductListResponse = Product;

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

  async createProduct(data: CreateFullProductRequest): Promise<Product> {
    const response = await fetch(`${API_URL}/products`, {
      method: "POST",
      headers: this.getHeaders(),
      body: JSON.stringify(data),
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: "Failed to create product" }));
      throw new Error(error.message || "Failed to create product");
    }

    return response.json();
  }

  async updateProduct(id: number, data: UpdateProductRequest): Promise<Product> {
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
      const error = await response.json().catch(() => ({ message: "Failed to update product" }));
      throw new Error(error.message || "Failed to update product");
    }

    return response.json();
  }

  async getAllProducts(): Promise<ProductListResponse[]> {
    const response = await fetch(`${API_URL}/products`, {
      headers: this.getHeaders(),
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: "Failed to fetch products" }));
      throw new Error(error.message || "Failed to fetch products");
    }

    return response.json();
  }

  async getProductById(id: number): Promise<Product> {
    const response = await fetch(`${API_URL}/products/${id}`, {
      headers: this.getHeaders(),
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: "Failed to fetch product" }));
      throw new Error(error.message || "Failed to fetch product");
    }

    return response.json();
  }

  async deleteProduct(id: number): Promise<void> {
    const response = await fetch(`${API_URL}/products/${id}`, {
      method: "DELETE",
      headers: this.getHeaders(),
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: "Failed to delete product" }));
      throw new Error(error.message || "Failed to delete product");
    }
  }
}

export const ProductManagementClientInstance = new ProductManagementClient();
