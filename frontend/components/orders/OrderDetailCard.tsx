import { AdminPayment, CompleteOrderResponse } from "@/lib/types";
import { formatUsd } from "@/lib/currency";
import styles from "@/styles/orders.module.css";

interface OrderDetailCardProps {
  order: CompleteOrderResponse;
  title?: string;
  showAdminPayments?: boolean;
}

export default function OrderDetailCard({
  order,
  title = "Order details",
  showAdminPayments = false,
}: OrderDetailCardProps) {
  const payments = showAdminPayments ? order.adminPayments ?? [] : order.customerPayments ?? [];

  return (
    <section className={styles.detailCard}>
      <div className={styles.detailHeader}>
        <div>
          <p className={styles.eyebrow}>{title}</p>
          <h2 className={styles.detailTitle}>{order.order.customerOrderId}</h2>
        </div>
        <div className={styles.statusGroup}>
          <span className={styles.statusPill}>{order.order.orderStatus}</span>
          <span className={styles.statusPillMuted}>{order.order.paymentStatus}</span>
        </div>
      </div>

      <div className={styles.detailGrid}>
        <div className={styles.infoPanel}>
          <h3>Summary</h3>
          <p>{order.order.customerName}</p>
          <p>{order.order.customerEmail}</p>
          <p>{order.order.customerPhone || "No phone provided"}</p>
          <p>Total: {formatUsd(order.order.totalAmount)}</p>
          <p>Placed: {new Date(order.order.createdAt).toLocaleString()}</p>
          {order.order.notes ? <p>Notes: {order.order.notes}</p> : null}
        </div>

        <div className={styles.infoPanel}>
          <h3>Shipping</h3>
          {order.addresses.length === 0 ? (
            <p>No address found.</p>
          ) : (
            order.addresses.map((address) => (
              <div key={address.orderAddressesId} className={styles.stack}>
                <strong>{address.type}</strong>
                <span>{address.line1}</span>
                {address.line2 ? <span>{address.line2}</span> : null}
                <span>
                  {address.city}, {address.state} {address.pincode}
                </span>
                <span>{address.country}</span>
              </div>
            ))
          )}
        </div>
      </div>

      <div className={styles.sectionBlock}>
        <h3>Items</h3>
        <div className={styles.itemsList}>
          {order.items.map((item) => (
            <article key={item.orderItemsId} className={styles.itemCard}>
              <div className={styles.itemHeader}>
                <div>
                  <strong>{item.productName}</strong>
                  <p>SKU {item.sku}</p>
                </div>
                <div className={styles.alignRight}>
                  <strong>{formatUsd(item.totalPrice)}</strong>
                  <p>Qty {item.quantity}</p>
                </div>
              </div>
              {item.customizations.length > 0 ? (
                <div className={styles.tags}>
                  {item.customizations.map((customization) => (
                    <span key={customization.orderItemCustomizationsId} className={styles.tag}>
                      {customization.type}: {customization.value}
                    </span>
                  ))}
                </div>
              ) : (
                <p className={styles.muted}>No customizations selected.</p>
              )}
            </article>
          ))}
        </div>
      </div>

      {order.prescriptions.length > 0 ? (
        <div className={styles.sectionBlock}>
          <h3>Prescription</h3>
          {order.prescriptions.map((prescription) => (
            <div key={prescription.customerPrescriptionsId} className={styles.infoPanel}>
              <p>Right Sphere: {prescription.rightSphere ?? "-"}</p>
              <p>Right Cylinder: {prescription.rightCylinder ?? "-"}</p>
              <p>Right Axis: {prescription.rightAxis ?? "-"}</p>
              <p>Left Sphere: {prescription.leftSphere ?? "-"}</p>
              <p>Left Cylinder: {prescription.leftCylinder ?? "-"}</p>
              <p>Left Axis: {prescription.leftAxis ?? "-"}</p>
              <p>PD: {prescription.pd ?? "-"}</p>
              {prescription.notes ? <p>Notes: {prescription.notes}</p> : null}
            </div>
          ))}
        </div>
      ) : null}

      <div className={styles.sectionBlock}>
        <h3>Timeline</h3>
        <div className={styles.timeline}>
          {order.statusLogs.map((log) => (
            <div key={log.orderStatusLogsId} className={styles.timelineItem}>
              <strong>{log.status}</strong>
              <span>{new Date(log.createdAt).toLocaleString()}</span>
              {log.comment ? <p>{log.comment}</p> : null}
            </div>
          ))}
        </div>
      </div>

      <div className={styles.sectionBlock}>
        <h3>{showAdminPayments ? "Payments" : "Payment snapshot"}</h3>
        <div className={styles.itemsList}>
          {payments.map((payment, index) => (
            <article
              key={`${payment.createdAt}-${index}`}
              className={styles.itemCard}
            >
              <div className={styles.itemHeader}>
                <div>
                  <strong>{payment.method}</strong>
                  <p>{payment.status}</p>
                </div>
                <div className={styles.alignRight}>
                  <strong>{formatUsd(payment.amount)}</strong>
                  <p>{new Date(payment.createdAt).toLocaleString()}</p>
                </div>
              </div>
              {showAdminPayments && (payment as AdminPayment).transactionId ? (
                <p className={styles.muted}>Txn: {(payment as AdminPayment).transactionId}</p>
              ) : null}
            </article>
          ))}
          {payments.length === 0 ? <p className={styles.muted}>No payment records available.</p> : null}
        </div>
      </div>
    </section>
  );
}
