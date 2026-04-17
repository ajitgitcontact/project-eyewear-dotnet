"use client";

import Link from "next/link";
import { useState } from "react";
import { useAuth } from "@/context/AuthContext";
import { usePathname, useRouter } from "next/navigation";
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
  const { user, logout } = useAuth();
  const router = useRouter();
  const pathname = usePathname();
  const [currency, setCurrency] = useState("GBP");

  const handleLogout = () => {
    logout();
    router.push("/login");
  };

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
            <option value="GBP">GBP</option>
            <option value="USD">USD</option>
            <option value="INR">INR</option>
          </select>

          {user ? (
            <div className={styles.userActions}>
              <div className={styles.userInfo}>
                <span className={styles.userName}>{user.name}</span>
              </div>
              {(user.role === "ADMIN" || user.role === "SUPER_ADMIN") && (
                <div className={styles.adminMenu}>
                  <Link href="/admin/users" className={styles.iconButton}>
                    Users
                  </Link>
                  <Link href="/admin/products" className={styles.iconButton}>
                    Products
                  </Link>
                </div>
              )}
              <button onClick={handleLogout} className={styles.iconButton}>
                Logout
              </button>
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
