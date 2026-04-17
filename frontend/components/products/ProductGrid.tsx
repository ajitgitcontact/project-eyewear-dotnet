"use client";

import { useEffect, useMemo, useRef, useState } from "react";
import Link from "next/link";
import { Product } from "@/lib/types";
import styles from "@/styles/productgrid.module.css";

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5047/api";

export default function ProductGrid() {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [sortBy, setSortBy] = useState("default");
  const [searchTerm, setSearchTerm] = useState("");
  const categoryRefs = useRef<Record<string, HTMLDivElement | null>>({});

  useEffect(() => {
    const fetchProducts = async () => {
      try {
        setLoading(true);
        const response = await fetch(`${API_BASE_URL}/Products?sort=${sortBy}`, {
          cache: "no-store",
        });

        if (!response.ok) {
          throw new Error(`HTTP ${response.status}`);
        }

        const data = await response.json();
        setProducts(data);
        setError(null);
      } catch (err) {
        console.error("Error fetching products:", err);
        setError("Failed to load products. Please try again later.");
      } finally {
        setLoading(false);
      }
    };

    fetchProducts();
  }, [sortBy]);

  const filteredProducts = useMemo(() => {
    const query = searchTerm.trim().toLowerCase();

    return products.filter((product) => {
      if (!query) {
        return true;
      }

      return (
        product.name.toLowerCase().includes(query) ||
        product.sku.toLowerCase().includes(query) ||
        product.category.toLowerCase().includes(query) ||
        (product.brand ?? "").toLowerCase().includes(query)
      );
    });
  }, [products, searchTerm]);

  const groupedProducts = useMemo(() => {
    const groups = new Map<string, Product[]>();

    filteredProducts.forEach((product) => {
      const existing = groups.get(product.category) ?? [];
      existing.push(product);
      groups.set(product.category, existing);
    });

    return Array.from(groups.entries()).map(([category, items]) => ({
      category,
      items,
    }));
  }, [filteredProducts]);

  const scrollCategory = (category: string, direction: "left" | "right") => {
    const node = categoryRefs.current[category];

    if (!node) {
      return;
    }

    const amount = Math.max(node.clientWidth * 0.8, 260);
    node.scrollBy({
      left: direction === "left" ? -amount : amount,
      behavior: "smooth",
    });
  };

  return (
    <div className={styles.container}>
      <div className={styles.sortBar}>
        <h2>Products</h2>
        <div className={styles.controls}>
          <div className={styles.searchBox}>
            <input
              type="text"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              placeholder="Search by name, SKU, category, or brand"
              className={styles.searchInput}
            />
          </div>
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
      </div>

      <p className={styles.mobileHint}>Swipe left to browse more products</p>

      {loading && (
        <div className={styles.loading}>
          <p>Loading products...</p>
        </div>
      )}

      {error && (
        <div className={styles.error}>
          <p>{error}</p>
        </div>
      )}

      {!loading && !error && groupedProducts.length > 0 && (
        <div className={styles.categorySections}>
          {groupedProducts.map(({ category, items }) => (
            <section key={category} className={styles.categorySection}>
              <div className={styles.categoryHeader}>
                <div>
                  <p className={styles.categoryEyebrow}>Category</p>
                  <h3 className={styles.categoryTitle}>{category}</h3>
                </div>
                <div className={styles.categoryActions}>
                  <span className={styles.categoryCount}>{items.length} products</span>
                </div>
              </div>

              <div className={styles.sliderShell}>
                <button
                  type="button"
                  className={`${styles.scrollButton} ${styles.scrollButtonLeft}`}
                  onClick={() => scrollCategory(category, "left")}
                  aria-label={`Scroll ${category} left`}
                >
                  &larr;
                </button>

                <div
                  className={styles.slider}
                  ref={(node) => {
                    categoryRefs.current[category] = node;
                  }}
                >
                  {items.map((product) => (
                    <div key={product.productId} className={styles.productCard}>
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

                      <div className={styles.info}>
                        <h3 className={styles.name}>{product.name}</h3>
                        <p className={styles.sku}>SKU: {product.sku}</p>

                        {product.description && (
                          <p className={styles.description}>{product.description}</p>
                        )}

                        <div className={styles.pricing}>
                          <span className={styles.price}>${product.basePrice.toFixed(2)}</span>
                          <span className={styles.available}>
                            {product.availableQuantity > 0
                              ? `${product.availableQuantity} in stock`
                              : "Out of Stock"}
                          </span>
                        </div>

                        <Link href={`/product/${product.productId}`} className={styles.viewButton}>
                          View Details
                        </Link>
                      </div>
                    </div>
                  ))}
                </div>

                <button
                  type="button"
                  className={`${styles.scrollButton} ${styles.scrollButtonRight}`}
                  onClick={() => scrollCategory(category, "right")}
                  aria-label={`Scroll ${category} right`}
                >
                  &rarr;
                </button>
              </div>
            </section>
          ))}
        </div>
      )}

      {!loading && !error && groupedProducts.length === 0 && (
        <div className={styles.empty}>
          <p>No products found.</p>
        </div>
      )}
    </div>
  );
}
