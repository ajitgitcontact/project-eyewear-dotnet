"use client";

import { useEffect, useState } from "react";
import { Product } from "@/lib/types";
import styles from "@/styles/productgrid.module.css";

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5047/api";

export default function ProductGrid() {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [sortBy, setSortBy] = useState("default");

  useEffect(() => {
    const fetchProducts = async () => {
      try {
        setLoading(true);
        const response = await fetch(
          `${API_BASE_URL}/Products?sort=${sortBy}`,
          { cache: "no-store" }
        );

        if (!response.ok) {
          throw new Error(`HTTP ${response.status}`);
        }

        const data = await response.json();
        setProducts(data);
        setError(null);
      } catch (err) {
        console.error("❌ Error fetching products:", err);
        setError("Failed to load products. Please try again later.");
      } finally {
        setLoading(false);
      }
    };

    fetchProducts();
  }, [sortBy]);

  return (
    <div className={styles.container}>
      {/* Sort Options */}
      <div className={styles.sortBar}>
        <h2>Products</h2>
        <div className={styles.sortOptions}>
          <label htmlFor="sort">Sort by:</label>
          <select
            id="sort"
            value={sortBy}
            onChange={(e) => setSortBy(e.target.value)}
            className={styles.sortSelect}
          >
            <option value="default">Featured</option>
            <option value="price_asc">Price: Low to High</option>
            <option value="price_desc">Price: High to Low</option>
            <option value="newest">What's New</option>
            <option value="popularity">Most Popular</option>
          </select>
        </div>
      </div>

      {/* Loading State */}
      {loading && (
        <div className={styles.loading}>
          <p>Loading products...</p>
        </div>
      )}

      {/* Error State */}
      {error && (
        <div className={styles.error}>
          <p>{error}</p>
        </div>
      )}

      {/* Products Grid */}
      {!loading && !error && products.length > 0 && (
        <div className={styles.grid}>
          {products.map((product) => (
            <div key={product.productId} className={styles.productCard}>
              {/* Primary Image */}
              <div className={styles.imageContainer}>
                {product.images.length > 0 ? (
                  <img
                    src={product.images[0].imageUrl}
                    alt={product.name}
                    className={styles.image}
                  />
                ) : (
                  <div className={styles.placeholderImage}>No Image</div>
                )}
              </div>

              {/* Product Info */}
              <div className={styles.info}>
                <h3 className={styles.name}>{product.name}</h3>
                <p className={styles.sku}>SKU: {product.sku}</p>
                
                {product.description && (
                  <p className={styles.description}>{product.description}</p>
                )}

                <div className={styles.pricing}>
                  <span className={styles.price}>
                    ${product.basePrice.toFixed(2)}
                  </span>
                  <span className={styles.available}>
                    {product.availableQuantity > 0
                      ? `${product.availableQuantity} in stock`
                      : "Out of Stock"}
                  </span>
                </div>

                <button className={styles.viewButton}>
                  View Details
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* No Products State */}
      {!loading && !error && products.length === 0 && (
        <div className={styles.empty}>
          <p>No products found.</p>
        </div>
      )}
    </div>
  );
}
