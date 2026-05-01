"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useAuth } from "@/context/AuthContext";
import { useWishlist } from "@/context/WishlistContext";
import { ProductManagementClientInstance } from "@/lib/api/productManagementClient";
import { formatUsd } from "@/lib/currency";
import { Product } from "@/lib/types";
import styles from "@/styles/orders.module.css";

export default function WishlistView() {
  const { isAuthenticated, isLoading } = useAuth();
  const { productIds, toggleWishlist, clearWishlist } = useWishlist();
  const router = useRouter();
  const [products, setProducts] = useState<Product[]>([]);
  const [loadingProducts, setLoadingProducts] = useState(true);

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      router.replace("/login");
    }
  }, [isAuthenticated, isLoading, router]);

  useEffect(() => {
    if (!isAuthenticated) {
      return;
    }

    const loadProducts = async () => {
      try {
        setLoadingProducts(true);
        const fetched = await Promise.all(
          productIds.map((productId) => ProductManagementClientInstance.getProductById(productId)),
        );
        setProducts(fetched);
      } finally {
        setLoadingProducts(false);
      }
    };

    void loadProducts();
  }, [isAuthenticated, productIds]);

  if (isLoading || !isAuthenticated) {
    return <main className={styles.pageState}>Loading your wishlist...</main>;
  }

  return (
    <main className={styles.page}>
      <section className={styles.pageHeading}>
        <div>
          <p className={styles.eyebrow}>Wishlist</p>
          <h1>Products you saved for later</h1>
          <p className={styles.lead}>Keep a shortlist of frames you want to compare or order next.</p>
        </div>
        {products.length > 0 ? (
          <button type="button" className={styles.secondaryButton} onClick={clearWishlist}>
            Clear wishlist
          </button>
        ) : null}
      </section>

      {loadingProducts ? (
        <div className={styles.pageState}>Loading wishlist items...</div>
      ) : products.length === 0 ? (
        <div className={styles.pageState}>Your wishlist is empty right now.</div>
      ) : (
        <div className={styles.wishlistGrid}>
          {products.map((product) => {
            const primaryImage = product.images.find((image) => image.isPrimary) ?? product.images[0];

            return (
              <article key={product.productId} className={styles.wishlistCard}>
                {primaryImage ? (
                  <img src={primaryImage.imageUrl} alt={product.name} className={styles.wishlistImage} />
                ) : (
                  <div className={styles.wishlistImagePlaceholder}>No image</div>
                )}
                <div className={styles.wishlistBody}>
                  <div>
                    <h2>{product.name}</h2>
                    <p>{product.category}</p>
                    <p>SKU {product.sku}</p>
                  </div>
                  <strong>{formatUsd(product.basePrice)}</strong>
                  <div className={styles.inlineActions}>
                    <Link href={`/product/${product.productId}`} className="button">
                      View and order
                    </Link>
                    <button
                      type="button"
                      className={styles.secondaryButton}
                      onClick={() => toggleWishlist(product.productId)}
                    >
                      Remove
                    </button>
                  </div>
                </div>
              </article>
            );
          })}
        </div>
      )}
    </main>
  );
}
