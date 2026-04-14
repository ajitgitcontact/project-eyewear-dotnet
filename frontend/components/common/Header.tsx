"use client";

import { useState } from "react";
import { useAuth } from "@/context/AuthContext";
import { useRouter } from "next/navigation";
import styles from "@/styles/header.module.css";

const categories = [
  { name: "New", subcategories: ["Latest Arrivals", "Trending", "Hot Deals"] },
  { name: "In", subcategories: ["Featured", "Popular", "Best Sellers"] },
  { name: "Sunglasses", subcategories: ["UV Protection", "Polarized", "Designer", "Sports"] },
  { name: "Anti Blue Lens", subcategories: ["Gaming", "Office Work", "All Day Wear"] },
  { name: "Clearance", subcategories: ["Up to 50% Off", "Last Chance", "Overstock"] },
  { name: "Coming Soon", subcategories: ["Summer Collection", "Designer Brands", "New Tech"] },
  { name: "Accessories", subcategories: ["Cases", "Cleaning Kits", "Straps", "Lens Wipes"] },
];

export default function Header() {
  const { user, logout } = useAuth();
  const router = useRouter();
  const [activeCategory, setActiveCategory] = useState<string | null>(null);

  const handleLogout = () => {
    logout();
    router.push("/login");
  };

  if (!user) {
    return null; // Don't show header if not logged in
  }

  return (
    <header className={styles.header}>
      <div className={styles.container}>
        {/* Logo */}
        <div className={styles.logo}>
          <h1>Eyewear Store</h1>
        </div>

        {/* Navigation */}
        <nav className={styles.nav}>
          {categories.map((category) => (
            <div
              key={category.name}
              className={styles.navItem}
              onMouseEnter={() => setActiveCategory(category.name)}
              onMouseLeave={() => setActiveCategory(null)}
            >
              <button className={styles.navButton}>
                {category.name}
              </button>
              
              {/* Dropdown Menu */}
              {activeCategory === category.name && (
                <div className={styles.dropdown}>
                  {category.subcategories.map((sub) => (
                    <a key={sub} href="#" className={styles.dropdownItem}>
                      {sub}
                    </a>
                  ))}
                </div>
              )}
            </div>
          ))}
        </nav>

        {/* User Section */}
        <div className={styles.userSection}>
          <div className={styles.userInfo}>
            <span className={styles.userName}>{user.name}</span>
            <span className={styles.userRole}>{user.role}</span>
          </div>
          {(user.role === "ADMIN" || user.role === "SUPER_ADMIN") && (
            <div className={styles.adminMenu}>
              <button
                onClick={() => router.push("/admin/users")}
                className={styles.adminLink}
                title="User Management"
              >
                Users
              </button>
              <button
                onClick={() => router.push("/admin/products")}
                className={styles.adminLink}
                title="Product Management"
              >
                Products
              </button>
            </div>
          )}
          <button onClick={handleLogout} className={styles.logoutBtn}>
            Logout
          </button>
        </div>
      </div>
    </header>
  );
}
