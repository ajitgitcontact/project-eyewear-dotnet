import { AuthClient } from "@/lib/api/authClient";
import {
  CompleteOrderResponse,
  CustomerOrderListResponse,
  OrderCreationRequest,
  OrderCreationResponse,
  OrderSearchResponse,
} from "@/lib/types";

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5047/api";

export interface CustomerOrdersQuery {
  fromCreatedDate?: string;
  toCreatedDate?: string;
  orderStatus?: string;
  paymentStatus?: string;
  pageNumber?: number;
  pageSize?: number;
}

export interface AdminOrdersQuery extends CustomerOrdersQuery {
  customerOrderId?: string;
  email?: string;
  contactNumber?: string;
  userId?: number;
}

function buildQueryString(params: Record<string, string | number | undefined>) {
  const search = new URLSearchParams();

  Object.entries(params).forEach(([key, value]) => {
    if (value === undefined || value === "") {
      return;
    }

    search.set(key, String(value));
  });

  const result = search.toString();
  return result ? `?${result}` : "";
}

async function parseError(response: Response, fallback: string) {
  const data = await response.json().catch(() => null);
  return data?.message || data?.title || fallback;
}

class OrderClient {
  private getHeaders() {
    const headers: Record<string, string> = {
      "Content-Type": "application/json",
    };
    const token = AuthClient.getToken();

    if (token) {
      headers.Authorization = `Bearer ${token}`;
    }

    return headers;
  }

  async createOrder(payload: OrderCreationRequest): Promise<OrderCreationResponse> {
    const response = await fetch(`${API_BASE_URL}/orders/create`, {
      method: "POST",
      headers: this.getHeaders(),
      body: JSON.stringify(payload),
    });

    if (!response.ok) {
      throw new Error(await parseError(response, "Failed to create order."));
    }

    return response.json();
  }

  async getCustomerOrders(query: CustomerOrdersQuery = {}): Promise<CustomerOrderListResponse> {
    const response = await fetch(
      `${API_BASE_URL}/customer/orders${buildQueryString({
        fromCreatedDate: query.fromCreatedDate,
        toCreatedDate: query.toCreatedDate,
        orderStatus: query.orderStatus,
        paymentStatus: query.paymentStatus,
        pageNumber: query.pageNumber ?? 1,
        pageSize: query.pageSize ?? 20,
      })}`,
      {
        headers: this.getHeaders(),
        cache: "no-store",
      },
    );

    if (!response.ok) {
      throw new Error(await parseError(response, "Failed to fetch customer orders."));
    }

    return response.json();
  }

  async getOrderByCustomerOrderId(customerOrderId: string): Promise<CompleteOrderResponse> {
    const response = await fetch(`${API_BASE_URL}/orders/${customerOrderId}`, {
      headers: this.getHeaders(),
      cache: "no-store",
    });

    if (!response.ok) {
      throw new Error(await parseError(response, "Failed to fetch order details."));
    }

    return response.json();
  }

  async searchOrders(query: AdminOrdersQuery = {}): Promise<OrderSearchResponse> {
    const response = await fetch(
      `${API_BASE_URL}/orders${buildQueryString({
        fromCreatedDate: query.fromCreatedDate,
        toCreatedDate: query.toCreatedDate,
        orderStatus: query.orderStatus,
        paymentStatus: query.paymentStatus,
        customerOrderId: query.customerOrderId,
        email: query.email,
        contactNumber: query.contactNumber,
        userId: query.userId,
        pageNumber: query.pageNumber ?? 1,
        pageSize: query.pageSize ?? 20,
      })}`,
      {
        headers: this.getHeaders(),
        cache: "no-store",
      },
    );

    if (!response.ok) {
      throw new Error(await parseError(response, "Failed to search orders."));
    }

    return response.json();
  }
}

export const OrderClientInstance = new OrderClient();
