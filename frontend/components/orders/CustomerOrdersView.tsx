"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/context/AuthContext";
import { OrderClientInstance } from "@/lib/api/orderClient";
import { formatUsd } from "@/lib/currency";
import { CompleteOrderResponse, CustomerOrderListItem } from "@/lib/types";
import OrderDetailCard from "@/components/orders/OrderDetailCard";
import styles from "@/styles/orders.module.css";

export default function CustomerOrdersView() {
  const { isAuthenticated, isLoading } = useAuth();
  const router = useRouter();
  const [orders, setOrders] = useState<CustomerOrderListItem[]>([]);
  const [selectedId, setSelectedId] = useState<string>("");
  const [selectedOrder, setSelectedOrder] = useState<CompleteOrderResponse | null>(null);
  const [loadingOrders, setLoadingOrders] = useState(true);
  const [loadingDetail, setLoadingDetail] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      router.replace("/login");
    }
  }, [isAuthenticated, isLoading, router]);

  useEffect(() => {
    if (!isAuthenticated) {
      return;
    }

    const loadOrders = async () => {
      try {
        setLoadingOrders(true);
        const response = await OrderClientInstance.getCustomerOrders({ pageNumber: 1, pageSize: 20 });
        setOrders(response.orders);
        setSelectedId(response.orders[0]?.customerOrderId ?? "");
        setError(null);
      } catch (err) {
        setError(err instanceof Error ? err.message : "Failed to load your orders.");
      } finally {
        setLoadingOrders(false);
      }
    };

    loadOrders();
  }, [isAuthenticated]);

  useEffect(() => {
    if (!selectedId) {
      setSelectedOrder(null);
      return;
    }

    const loadDetail = async () => {
      try {
        setLoadingDetail(true);
        const response = await OrderClientInstance.getOrderByCustomerOrderId(selectedId);
        setSelectedOrder(response);
      } catch (err) {
        setError(err instanceof Error ? err.message : "Failed to load order details.");
      } finally {
        setLoadingDetail(false);
      }
    };

    loadDetail();
  }, [selectedId]);

  if (isLoading || !isAuthenticated) {
    return <main className={styles.pageState}>Loading your order workspace...</main>;
  }

  return (
    <main className={styles.page}>
      <section className={styles.pageHeading}>
        <div>
          <p className={styles.eyebrow}>Customer orders</p>
          <h1>Track and review every order</h1>
          <p className={styles.lead}>Pick one of your orders to see line items, address, payment snapshot, and status history.</p>
        </div>
      </section>

      {error ? <div className={styles.errorBox}>{error}</div> : null}

      <div className={styles.columns}>
        <section className={styles.listCard}>
          <div className={styles.cardHeader}>
            <h2>My orders</h2>
            <span>{orders.length} loaded</span>
          </div>

          {loadingOrders ? (
            <div className={styles.pageState}>Loading orders...</div>
          ) : orders.length === 0 ? (
            <div className={styles.pageState}>You have not placed any orders yet.</div>
          ) : (
            <div className={styles.orderList}>
              {orders.map((order) => (
                <button
                  type="button"
                  key={order.customerOrderId}
                  className={`${styles.orderRow} ${selectedId === order.customerOrderId ? styles.orderRowActive : ""}`}
                  onClick={() => setSelectedId(order.customerOrderId)}
                >
                  <div>
                    <strong>{order.customerOrderId}</strong>
                    <p>{new Date(order.createdAt).toLocaleString()}</p>
                  </div>
                  <div className={styles.alignRight}>
                    <strong>{formatUsd(order.totalAmount)}</strong>
                    <p>
                      {order.orderStatus} • {order.paymentStatus}
                    </p>
                  </div>
                </button>
              ))}
            </div>
          )}
        </section>

        <section className={styles.detailColumn}>
          {loadingDetail ? (
            <div className={styles.pageState}>Loading order detail...</div>
          ) : selectedOrder ? (
            <OrderDetailCard order={selectedOrder} title="Selected order" />
          ) : (
            <div className={styles.pageState}>Select an order to see the full breakdown.</div>
          )}
        </section>
      </div>
    </main>
  );
}
