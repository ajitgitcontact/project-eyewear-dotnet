export interface CustomizationImage {
  customizationImageId: number;
  productId: number;
  customizationOptionId: number;
  customizationValueId: number;
  imageUrl: string;
  createdAt: string;
}

export interface CustomizationValue {
  customizationValueId: number;
  customizationOptionId: number;
  value: string;
  additionalPrice: number;
  createdAt: string;
  customizationImages: CustomizationImage[];
}

export interface CustomizationOption {
  customizationOptionId: number;
  productId: number;
  name: string;
  isRequired: boolean;
  displayOrder: number;
  createdAt: string;
  values: CustomizationValue[];
}

export interface ProductImage {
  productImageId: number;
  productId: number;
  imageUrl: string;
  isPrimary: boolean;
  displayOrder: number;
  createdAt: string;
}

export interface Product {
  productId: number;
  sku: string;
  name: string;
  description?: string | null;
  brand?: string | null;
  category: string;
  basePrice: number;
  availableQuantity: number;
  soldQuantity: number;
  priority: number;
  hasPrescription: boolean;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string | null;
  customizationOptions: CustomizationOption[];
  images: ProductImage[];
}

// Authentication Types
export interface User {
  userId: number;
  name: string;
  email: string;
  role: "ADMIN" | "SUPER_ADMIN" | "CUSTOMER";
  createdAt: string;
}

export interface LoginResponse {
  token?: string;
  userId: number;
  name: string;
  email: string;
  role: "ADMIN" | "SUPER_ADMIN" | "CUSTOMER";
}

export interface LoginRequest {
  email: string;
  password: string;
}
