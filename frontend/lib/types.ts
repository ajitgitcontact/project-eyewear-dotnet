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

export type PaymentStatus = "PENDING" | "PAID" | "FAILED";
export type OrderStatus = "CREATED" | "CONFIRMED" | "SHIPPED" | "DELIVERED" | "CANCELLED";
export type PaymentMethod = "COD" | "PREPAID" | "UPI" | "CARD";
export type PaymentTxnStatus = "INITIATED" | "SUCCESS" | "FAILED";
export type AddressType = "SHIPPING" | "BILLING";

export interface OrderCreationCustomizationRequest {
  customizationOptionId?: number;
  customizationValueId?: number;
  type?: string;
  value?: string;
}

export interface OrderCreationPrescriptionRequest {
  rightSphere?: number;
  rightCylinder?: number;
  rightAxis?: number;
  rightAdd?: number;
  leftSphere?: number;
  leftCylinder?: number;
  leftAxis?: number;
  leftAdd?: number;
  pd?: number;
  notes?: string;
}

export interface OrderCreationRequest {
  customer: {
    name: string;
    email: string;
    phone?: string;
  };
  address: {
    type: AddressType;
    line1: string;
    line2?: string;
    city: string;
    state: string;
    pincode: string;
    country: string;
  };
  items: Array<{
    productId: number;
    quantity: number;
    customizations: OrderCreationCustomizationRequest[];
  }>;
  prescription?: OrderCreationPrescriptionRequest;
  payment: {
    method: PaymentMethod;
    transactionId?: string;
  };
  notes?: string;
  couponCode?: string;
  discountCode?: string;
}

export interface OrderCreationResponse {
  customerOrderId: string;
  subtotal: number;
  discountAmount: number;
  finalAmount: number;
}

export interface CustomerOrderListItem {
  customerOrderId: string;
  totalAmount: number;
  paymentStatus: PaymentStatus;
  orderStatus: OrderStatus;
  createdAt: string;
  updatedAt: string;
}

export interface CustomerOrderListResponse {
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  orders: CustomerOrderListItem[];
}

export interface OrderSearchItem {
  ordersId: string;
  customerOrderId: string;
  userId: number;
  customerName: string;
  customerEmail: string;
  customerPhone?: string | null;
  totalAmount: number;
  paymentStatus: PaymentStatus;
  orderStatus: OrderStatus;
  createdAt: string;
  updatedAt: string;
}

export interface OrderSearchResponse {
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  orders: OrderSearchItem[];
}

export interface CompleteOrderCustomization {
  orderItemCustomizationsId: string;
  orderItemId: string;
  customizationOptionId?: number | null;
  customizationValueId?: number | null;
  type: string;
  value: string;
  createdAt: string;
  updatedAt: string;
}

export interface CompleteOrderItem {
  orderItemsId: string;
  customerOrderId: string;
  productId: number;
  sku: string;
  productName: string;
  quantity: number;
  price: number;
  totalPrice: number;
  createdAt: string;
  updatedAt: string;
  customizations: CompleteOrderCustomization[];
}

export interface CompleteOrderAddress {
  orderAddressesId: string;
  customerOrderId: string;
  type: AddressType;
  line1: string;
  line2?: string | null;
  city: string;
  state: string;
  pincode: string;
  country: string;
  createdAt: string;
  updatedAt: string;
}

export interface CompleteOrderPrescription {
  customerPrescriptionsId: string;
  userId: number;
  customerOrderId: string;
  rightSphere?: number | null;
  rightCylinder?: number | null;
  rightAxis?: number | null;
  rightAdd?: number | null;
  leftSphere?: number | null;
  leftCylinder?: number | null;
  leftAxis?: number | null;
  leftAdd?: number | null;
  pd?: number | null;
  notes?: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CustomerSafePayment {
  method: PaymentMethod;
  status: PaymentTxnStatus;
  amount: number;
  createdAt: string;
}

export interface AdminPayment extends CustomerSafePayment {
  paymentsId: string;
  customerOrderId: string;
  transactionId?: string | null;
  updatedAt: string;
}

export interface CompleteOrderStatusLog {
  orderStatusLogsId: string;
  customerOrderId: string;
  status: OrderStatus;
  comment?: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CompleteOrderResponse {
  order: {
    ordersId: string;
    customerOrderId: string;
    userId: number;
    customerName: string;
    customerEmail: string;
    customerPhone?: string | null;
    totalAmount: number;
    paymentStatus: PaymentStatus;
    orderStatus: OrderStatus;
    notes?: string | null;
    createdAt: string;
    updatedAt: string;
  };
  items: CompleteOrderItem[];
  addresses: CompleteOrderAddress[];
  prescriptions: CompleteOrderPrescription[];
  statusLogs: CompleteOrderStatusLog[];
  customerPayments?: CustomerSafePayment[] | null;
  adminPayments?: AdminPayment[] | null;
}
