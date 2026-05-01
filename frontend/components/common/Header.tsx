"use client";

import Link from "next/link";
import { useState } from "react";
import { useAuth } from "@/context/AuthContext";
import { useWishlist } from "@/context/WishlistContext";
import { usePathname } from "next/navigation";
import styles from "@/styles/header.module.css";

const primaryLinks = [
  { name: "Sunglasses", href: "/" },
  { name: "Anti Blue Lens", href: "/" },
  { name: "Clearance", href: "/" },
  { name: "Coming Soon", href: "/" },
  { name: "Accessories", href: "/" },
];

const secondaryLinks = ["Transition Lenses", "Apparel", "Virtual Try On"];

export default function Header() {
  const { user } = useAuth();
  const { count } = useWishlist();
  const pathname = usePathname();
  const [currency, setCurrency] = useState("USD");

  return (
    <header className={styles.header}>
      <div className={styles.utilityBar}>
        <div className={styles.utilityInner}>
          <div className={styles.utilityLinks}>
            <span>B2B</span>
            <span>Our Stores</span>
          </div>
          <div className={styles.utilityCenter}>Virtual Try On</div>
          <div className={styles.utilityLinks}>
            <span>Reviews</span>
            <span>Worldwide Shipping</span>
          </div>
        </div>
      </div>

      <div className={styles.mainBar}>
        <div className={styles.brandBlock}>
          <Link href="/" className={styles.brandMark}>
              <span className={styles.brandName}>Eyewear Co.</span>
          </Link>
        </div>

        <nav className={styles.nav} aria-label="Primary">
          {primaryLinks.map((link) => (
            <Link
              key={link.name}
              href={link.href}
              className={`${styles.navLink} ${pathname === link.href ? styles.navLinkActive : ""}`}
            >
              {link.name}
            </Link>
          ))}
        </nav>

        <div className={styles.actions}>
          <select
            aria-label="Currency"
            className={styles.currencySelect}
            value={currency}
            onChange={(e) => setCurrency(e.target.value)}
          >
            <option value="USD">USD</option>
          </select>

          {user ? (
            <div className={styles.userActions}>
              <Link href="/account" className={styles.userInfoLink}>
                <span className={styles.userIcon} aria-hidden="true">
                  <span className={styles.userIconHead} />
                  <span className={styles.userIconBody} />
                </span>
                <div className={styles.userInfo}>
                  <span className={styles.userTabLabel}>User</span>
                  <span className={styles.userName}>{user.name}</span>
                </div>
              </Link>
              <Link href="/wishlist" className={styles.iconButton}>
                Wishlist ({count})
              </Link>
              <Link href="/orders" className={styles.iconButton}>
                My Orders
              </Link>
              {(user.role === "ADMIN" || user.role === "SUPER_ADMIN") && (
                <div className={styles.adminMenu}>
                  <Link href="/admin/users" className={styles.iconButton}>
                    Users
                  </Link>
                  <Link href="/admin/products" className={styles.iconButton}>
                    Products
                  </Link>
                  <Link href="/admin/orders" className={styles.iconButton}>
                    Orders
                  </Link>
                </div>
              )}
            </div>
          ) : (
            <div className={styles.userActions}>
              <Link href="/login" className={styles.iconButton}>
                Login
              </Link>
              <Link href="/login" className={styles.iconButtonAccent}>
                Sign Up
              </Link>
            </div>
          )}
        </div>
      </div>

    
    </header>
  );
}
