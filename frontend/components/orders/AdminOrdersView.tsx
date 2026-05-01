"use client";

import { FormEvent, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/context/AuthContext";
import { AdminOrdersQuery, OrderClientInstance } from "@/lib/api/orderClient";
import { formatUsd } from "@/lib/currency";
import { CompleteOrderResponse, OrderSearchItem } from "@/lib/types";
import OrderDetailCard from "@/components/orders/OrderDetailCard";
import styles from "@/styles/orders.module.css";

const initialFilters: AdminOrdersQuery = {
  pageNumber: 1,
  pageSize: 20,
  customerOrderId: "",
  email: "",
  contactNumber: "",
  orderStatus: "",
  paymentStatus: "",
};

export default function AdminOrdersView() {
  const { user, isAuthenticated, isLoading } = useAuth();
  const router = useRouter();
  const [filters, setFilters] = useState<AdminOrdersQuery>(initialFilters);
  const [orders, setOrders] = useState<OrderSearchItem[]>([]);
  const [selectedOrder, setSelectedOrder] = useState<CompleteOrderResponse | null>(null);
  const [selectedId, setSelectedId] = useState("");
  const [loadingList, setLoadingList] = useState(true);
  const [loadingDetail, setLoadingDetail] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (isLoading) {
      return;
    }

    if (!isAuthenticated) {
      router.replace("/login");
      return;
    }

    if (user?.role !== "ADMIN" && user?.role !== "SUPER_ADMIN") {
      router.replace("/");
    }
  }, [isAuthenticated, isLoading, router, user?.role]);

  const searchOrders = async (query: AdminOrdersQuery) => {
    try {
      setLoadingList(true);
      const response = await OrderClientInstance.searchOrders(query);
      setOrders(response.orders);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to search orders.");
    } finally {
      setLoadingList(false);
    }
  };

  useEffect(() => {
    if (user?.role === "ADMIN" || user?.role === "SUPER_ADMIN") {
      searchOrders(initialFilters);
    }
  }, [user?.role]);

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    await searchOrders({ ...filters, pageNumber: 1 });
  };

  const loadDetail = async (customerOrderId: string) => {
    try {
      setSelectedId(customerOrderId);
      setLoadingDetail(true);
      const response = await OrderClientInstance.getOrderByCustomerOrderId(customerOrderId);
      setSelectedOrder(response);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load complete order.");
    } finally {
      setLoadingDetail(false);
    }
  };

  if (isLoading || !isAuthenticated || (user?.role !== "ADMIN" && user?.role !== "SUPER_ADMIN")) {
    return <main className={styles.pageState}>Loading admin order management...</main>;
  }

  return (
    <main className={styles.page}>
      <section className={styles.pageHeading}>
        <div>
          <p className={styles.eyebrow}>Admin orders</p>
          <h1>Search, filter, and inspect complete orders</h1>
          <p className={styles.lead}>Use the list API for filtering, then fetch one complete order by customer order id when you need the full payload.</p>
        </div>
      </section>

      {error ? <div className={styles.errorBox}>{error}</div> : null}

      <section className={styles.filterCard}>
        <form className={styles.filterForm} onSubmit={handleSubmit}>
          <input
            value={filters.customerOrderId ?? ""}
            onChange={(event) => setFilters((current) => ({ ...current, customerOrderId: event.target.value }))}
            placeholder="Customer order id"
          />
          <input
            value={filters.email ?? ""}
            onChange={(event) => setFilters((current) => ({ ...current, email: event.target.value }))}
            placeholder="Customer email"
          />
          <input
            value={filters.contactNumber ?? ""}
            onChange={(event) => setFilters((current) => ({ ...current, contactNumber: event.target.value }))}
            placeholder="Contact number"
          />
          <select
            value={filters.orderStatus ?? ""}
            onChange={(event) => setFilters((current) => ({ ...current, orderStatus: event.target.value }))}
          >
            <option value="">All order statuses</option>
            <option value="CREATED">Created</option>
            <option value="CONFIRMED">Confirmed</option>
            <option value="SHIPPED">Shipped</option>
            <option value="DELIVERED">Delivered</option>
            <option value="CANCELLED">Cancelled</option>
          </select>
          <select
            value={filters.paymentStatus ?? ""}
            onChange={(event) => setFilters((current) => ({ ...current, paymentStatus: event.target.value }))}
          >
            <option value="">All payment statuses</option>
            <option value="PENDING">Pending</option>
            <option value="PAID">Paid</option>
            <option value="FAILED">Failed</option>
          </select>
          <button type="submit" className="button">
            Search orders
          </button>
        </form>
      </section>

      <div className={styles.columns}>
        <section className={styles.listCard}>
          <div className={styles.cardHeader}>
            <h2>Search results</h2>
            <span>{orders.length} rows</span>
          </div>

          {loadingList ? (
            <div className={styles.pageState}>Loading order list...</div>
          ) : orders.length === 0 ? (
            <div className={styles.pageState}>No orders matched these filters.</div>
          ) : (
            <div className={styles.orderList}>
              {orders.map((order) => (
                <button
                  type="button"
                  key={order.ordersId}
                  className={`${styles.orderRow} ${selectedId === order.customerOrderId ? styles.orderRowActive : ""}`}
                  onClick={() => loadDetail(order.customerOrderId)}
                >
                  <div>
                    <strong>{order.customerOrderId}</strong>
                    <p>{order.customerName}</p>
                    <p>{order.customerEmail}</p>
                  </div>
                  <div className={styles.alignRight}>
                    <strong>{formatUsd(order.totalAmount)}</strong>
                    <p>{order.orderStatus}</p>
                    <p>{order.paymentStatus}</p>
                  </div>
                </button>
              ))}
            </div>
          )}
        </section>

        <section className={styles.detailColumn}>
          {loadingDetail ? (
            <div className={styles.pageState}>Loading complete order payload...</div>
          ) : selectedOrder ? (
            <OrderDetailCard order={selectedOrder} title="Complete order payload" showAdminPayments />
          ) : (
            <div className={styles.pageState}>Select an order from the list to fetch its complete details.</div>
          )}
        </section>
      </div>
    </main>
  );
}
