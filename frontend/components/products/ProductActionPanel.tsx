"use client";

import { FormEvent, useEffect, useMemo, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useAuth } from "@/context/AuthContext";
import { useWishlist } from "@/context/WishlistContext";
import { OrderClientInstance } from "@/lib/api/orderClient";
import { formatUsd } from "@/lib/currency";
import { OrderCreationRequest, PaymentMethod, Product } from "@/lib/types";
import styles from "@/styles/productactions.module.css";

interface ProductActionPanelProps {
  product: Product;
}

type PrescriptionForm = {
  rightSphere: string;
  rightCylinder: string;
  rightAxis: string;
  rightAdd: string;
  leftSphere: string;
  leftCylinder: string;
  leftAxis: string;
  leftAdd: string;
  pd: string;
  notes: string;
};

const initialPrescription: PrescriptionForm = {
  rightSphere: "",
  rightCylinder: "",
  rightAxis: "",
  rightAdd: "",
  leftSphere: "",
  leftCylinder: "",
  leftAxis: "",
  leftAdd: "",
  pd: "",
  notes: "",
};

export default function ProductActionPanel({ product }: ProductActionPanelProps) {
  const { user, isAuthenticated } = useAuth();
  const { isWishlisted, toggleWishlist } = useWishlist();
  const router = useRouter();
  const [quantity, setQuantity] = useState(1);
  const [paymentMethod, setPaymentMethod] = useState<PaymentMethod>("COD");
  const [customerName, setCustomerName] = useState(user?.name ?? "");
  const [customerEmail, setCustomerEmail] = useState(user?.email ?? "");
  const [customerPhone, setCustomerPhone] = useState("");
  const [line1, setLine1] = useState("");
  const [line2, setLine2] = useState("");
  const [city, setCity] = useState("");
  const [stateName, setStateName] = useState("");
  const [pincode, setPincode] = useState("");
  const [country, setCountry] = useState("India");
  const [notes, setNotes] = useState("");
  const [transactionId, setTransactionId] = useState("");
  const [includePrescription, setIncludePrescription] = useState(false);
  const [prescription, setPrescription] = useState<PrescriptionForm>(initialPrescription);
  const [selectedValues, setSelectedValues] = useState<Record<number, number | "">>({});
  const [submitting, setSubmitting] = useState(false);
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    setCustomerName(user?.name ?? "");
    setCustomerEmail(user?.email ?? "");
  }, [user?.email, user?.name]);

  useEffect(() => {
    const defaults: Record<number, number | ""> = {};
    product.customizationOptions.forEach((option) => {
      if (option.isRequired && option.values[0]) {
        defaults[option.customizationOptionId] = option.values[0].customizationValueId;
      }
    });
    setSelectedValues(defaults);
  }, [product.customizationOptions]);

  const selectedCustomizationValues = useMemo(
    () =>
      product.customizationOptions
        .map((option) =>
          option.values.find((value) => value.customizationValueId === selectedValues[option.customizationOptionId]),
        )
        .filter((value) => value !== undefined),
    [product.customizationOptions, selectedValues],
  );

  const totalAmount = useMemo(() => {
    const customizationTotal = selectedCustomizationValues.reduce(
      (sum, value) => sum + value.additionalPrice,
      0,
    );

    return (product.basePrice + customizationTotal) * quantity;
  }, [product.basePrice, quantity, selectedCustomizationValues]);

  const handleShare = async () => {
    const url =
      typeof window !== "undefined" ? `${window.location.origin}/product/${product.productId}` : "";

    try {
      if (navigator.share) {
        await navigator.share({
          title: product.name,
          text: `Check out ${product.name} on Eyewear Co.`,
          url,
        });
      } else if (navigator.clipboard) {
        await navigator.clipboard.writeText(url);
      }

      setMessage("Product link shared successfully.");
      setError(null);
    } catch {
      setError("Unable to share this product right now.");
      setMessage(null);
    }
  };

  const parseOptionalNumber = (value: string) => {
    if (!value.trim()) {
      return undefined;
    }

    return Number(value);
  };

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    if (!isAuthenticated) {
      router.push("/login");
      return;
    }

    if (product.availableQuantity <= 0) {
      setError("This product is currently out of stock.");
      setMessage(null);
      return;
    }

    for (const option of product.customizationOptions) {
      if (option.isRequired && !selectedValues[option.customizationOptionId]) {
        setError(`Please select ${option.name.toLowerCase()}.`);
        setMessage(null);
        return;
      }
    }

    const payload: OrderCreationRequest = {
      customer: {
        name: customerName,
        email: customerEmail,
        phone: customerPhone || undefined,
      },
      address: {
        type: "SHIPPING",
        line1,
        line2: line2 || undefined,
        city,
        state: stateName,
        pincode,
        country,
      },
      items: [
        {
          productId: product.productId,
          quantity,
          customizations: product.customizationOptions
            .map((option) => {
              const selectedValueId = selectedValues[option.customizationOptionId];
              const selectedValue = option.values.find(
                (value) => value.customizationValueId === selectedValueId,
              );

              if (!selectedValue) {
                return null;
              }

              return {
                customizationOptionId: option.customizationOptionId,
                customizationValueId: selectedValue.customizationValueId,
                type: option.name,
                value: selectedValue.value,
              };
            })
            .filter((item): item is NonNullable<typeof item> => item !== null),
        },
      ],
      payment: {
        method: paymentMethod,
        transactionId: paymentMethod === "COD" ? undefined : transactionId || undefined,
      },
      notes: notes || undefined,
      prescription: includePrescription
        ? {
            rightSphere: parseOptionalNumber(prescription.rightSphere),
            rightCylinder: parseOptionalNumber(prescription.rightCylinder),
            rightAxis: parseOptionalNumber(prescription.rightAxis),
            rightAdd: parseOptionalNumber(prescription.rightAdd),
            leftSphere: parseOptionalNumber(prescription.leftSphere),
            leftCylinder: parseOptionalNumber(prescription.leftCylinder),
            leftAxis: parseOptionalNumber(prescription.leftAxis),
            leftAdd: parseOptionalNumber(prescription.leftAdd),
            pd: parseOptionalNumber(prescription.pd),
            notes: prescription.notes || undefined,
          }
        : undefined,
    };

    try {
      setSubmitting(true);
      const response = await OrderClientInstance.createOrder(payload);
      setMessage(`Order created successfully. Customer order id: ${response.customerOrderId}`);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to create order.");
      setMessage(null);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <section className={styles.panel}>
      <div className={styles.header}>
        <div>
          <p className={styles.eyebrow}>Buy this product</p>
          <h2>Order, save, or share</h2>
        </div>
        <div className={styles.quickActions}>
          <button
            type="button"
            className={styles.secondaryButton}
            onClick={() => toggleWishlist(product.productId)}
          >
            {isWishlisted(product.productId) ? "Remove wishlist" : "Add to wishlist"}
          </button>
          <button type="button" className={styles.secondaryButton} onClick={handleShare}>
            Share product
          </button>
        </div>
      </div>

      <div className={styles.priceBanner}>
        <div>
          <span>Total preview</span>
          <strong>{formatUsd(totalAmount)}</strong>
        </div>
        <div>
          <span>Stock</span>
          <strong>{product.availableQuantity} available</strong>
        </div>
      </div>

      {!isAuthenticated ? (
        <div className={styles.noticeBox}>
          Please <Link href="/login">log in</Link> to place an order, manage wishlist, and track order history.
        </div>
      ) : null}

      {message ? <div className={styles.successBox}>{message}</div> : null}
      {error ? <div className={styles.errorBox}>{error}</div> : null}

      <form className={styles.form} onSubmit={handleSubmit}>
        <div className={styles.grid}>
          <label className={styles.field}>
            <span>Quantity</span>
            <input
              type="number"
              min={1}
              max={Math.max(1, product.availableQuantity)}
              value={quantity}
              onChange={(event) => setQuantity(Math.max(1, Number(event.target.value) || 1))}
            />
          </label>

          <label className={styles.field}>
            <span>Payment method</span>
            <select value={paymentMethod} onChange={(event) => setPaymentMethod(event.target.value as PaymentMethod)}>
              <option value="COD">Cash on delivery</option>
              <option value="PREPAID">Prepaid</option>
              <option value="UPI">UPI</option>
              <option value="CARD">Card</option>
            </select>
          </label>

          {paymentMethod !== "COD" ? (
            <label className={styles.field}>
              <span>Transaction id</span>
              <input
                type="text"
                value={transactionId}
                onChange={(event) => setTransactionId(event.target.value)}
                placeholder="Optional for now"
              />
            </label>
          ) : null}
        </div>

        {product.customizationOptions.length > 0 ? (
          <div className={styles.section}>
            <h3>Customizations</h3>
            <div className={styles.grid}>
              {product.customizationOptions.map((option) => (
                <label key={option.customizationOptionId} className={styles.field}>
                  <span>
                    {option.name} {option.isRequired ? "*" : "(optional)"}
                  </span>
                  <select
                    value={selectedValues[option.customizationOptionId] ?? ""}
                    onChange={(event) =>
                      setSelectedValues((current) => ({
                        ...current,
                        [option.customizationOptionId]: event.target.value
                          ? Number(event.target.value)
                          : "",
                      }))
                    }
                  >
                    {!option.isRequired ? <option value="">Skip this option</option> : null}
                    {option.values.map((value) => (
                      <option key={value.customizationValueId} value={value.customizationValueId}>
                        {value.value} (+{formatUsd(value.additionalPrice)})
                      </option>
                    ))}
                  </select>
                </label>
              ))}
            </div>
          </div>
        ) : null}

        <div className={styles.section}>
          <h3>Customer details</h3>
          <div className={styles.grid}>
            <label className={styles.field}>
              <span>Name</span>
              <input value={customerName} onChange={(event) => setCustomerName(event.target.value)} required />
            </label>
            <label className={styles.field}>
              <span>Email</span>
              <input
                type="email"
                value={customerEmail}
                onChange={(event) => setCustomerEmail(event.target.value)}
                required
              />
            </label>
            <label className={styles.field}>
              <span>Phone</span>
              <input value={customerPhone} onChange={(event) => setCustomerPhone(event.target.value)} />
            </label>
          </div>
        </div>

        <div className={styles.section}>
          <h3>Shipping address</h3>
          <div className={styles.grid}>
            <label className={styles.field}>
              <span>Line 1</span>
              <input value={line1} onChange={(event) => setLine1(event.target.value)} required />
            </label>
            <label className={styles.field}>
              <span>Line 2</span>
              <input value={line2} onChange={(event) => setLine2(event.target.value)} />
            </label>
            <label className={styles.field}>
              <span>City</span>
              <input value={city} onChange={(event) => setCity(event.target.value)} required />
            </label>
            <label className={styles.field}>
              <span>State</span>
              <input value={stateName} onChange={(event) => setStateName(event.target.value)} required />
            </label>
            <label className={styles.field}>
              <span>Pincode</span>
              <input value={pincode} onChange={(event) => setPincode(event.target.value)} required />
            </label>
            <label className={styles.field}>
              <span>Country</span>
              <input value={country} onChange={(event) => setCountry(event.target.value)} required />
            </label>
          </div>
        </div>

        <div className={styles.section}>
          <div className={styles.sectionHeader}>
            <h3>Prescription</h3>
            <label className={styles.checkboxRow}>
              <input
                type="checkbox"
                checked={includePrescription}
                onChange={(event) => setIncludePrescription(event.target.checked)}
              />
              <span>{product.hasPrescription ? "Add prescription details" : "Attach optional prescription"}</span>
            </label>
          </div>

          {includePrescription ? (
            <div className={styles.grid}>
              {Object.entries(initialPrescription).map(([key]) => (
                <label key={key} className={styles.field}>
                  <span>{key.replace(/([A-Z])/g, " $1").replace(/^./, (letter) => letter.toUpperCase())}</span>
                  {key === "notes" ? (
                    <textarea
                      value={prescription[key as keyof PrescriptionForm]}
                      onChange={(event) =>
                        setPrescription((current) => ({ ...current, [key]: event.target.value }))
                      }
                    />
                  ) : (
                    <input
                      value={prescription[key as keyof PrescriptionForm]}
                      onChange={(event) =>
                        setPrescription((current) => ({ ...current, [key]: event.target.value }))
                      }
                    />
                  )}
                </label>
              ))}
            </div>
          ) : null}
        </div>

        <div className={styles.section}>
          <label className={styles.field}>
            <span>Order notes</span>
            <textarea value={notes} onChange={(event) => setNotes(event.target.value)} />
          </label>
        </div>

        <div className={styles.footer}>
          <button type="submit" className="button" disabled={submitting || !isAuthenticated}>
            {submitting ? "Placing order..." : "Place order"}
          </button>
          <Link href="/orders" className={styles.secondaryButtonLink}>
            View my orders
          </Link>
        </div>
      </form>
    </section>
  );
}
