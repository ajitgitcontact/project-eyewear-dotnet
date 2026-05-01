"use client";

import React, { createContext, useContext, useEffect, useState } from "react";

const WISHLIST_STORAGE_KEY = "wishlistProductIds";

interface WishlistContextType {
  productIds: number[];
  count: number;
  isWishlisted: (productId: number) => boolean;
  toggleWishlist: (productId: number) => void;
  clearWishlist: () => void;
}

const WishlistContext = createContext<WishlistContextType | undefined>(undefined);

export function WishlistProvider({ children }: { children: React.ReactNode }) {
  const [productIds, setProductIds] = useState<number[]>([]);

  useEffect(() => {
    if (typeof window === "undefined") {
      return;
    }

    const stored = window.localStorage.getItem(WISHLIST_STORAGE_KEY);
    if (!stored) {
      return;
    }

    try {
      const parsed = JSON.parse(stored);
      if (Array.isArray(parsed)) {
        setProductIds(parsed.filter((item): item is number => typeof item === "number"));
      }
    } catch {
      window.localStorage.removeItem(WISHLIST_STORAGE_KEY);
    }
  }, []);

  const persist = (nextIds: number[]) => {
    setProductIds(nextIds);
    if (typeof window !== "undefined") {
      window.localStorage.setItem(WISHLIST_STORAGE_KEY, JSON.stringify(nextIds));
    }
  };

  const toggleWishlist = (productId: number) => {
    persist(
      productIds.includes(productId)
        ? productIds.filter((id) => id !== productId)
        : [...productIds, productId],
    );
  };

  const clearWishlist = () => persist([]);

  return (
    <WishlistContext.Provider
      value={{
        productIds,
        count: productIds.length,
        isWishlisted: (productId: number) => productIds.includes(productId),
        toggleWishlist,
        clearWishlist,
      }}
    >
      {children}
    </WishlistContext.Provider>
  );
}

export function useWishlist() {
  const context = useContext(WishlistContext);

  if (!context) {
    throw new Error("useWishlist must be used within WishlistProvider");
  }

  return context;
}
