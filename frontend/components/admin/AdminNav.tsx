"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import styles from "@/styles/adminnav.module.css";

export default function AdminNav() {
  const pathname = usePathname();

  const tabs = [
    { name: "User Management", path: "/admin/users" },
    { name: "Product Management", path: "/admin/products" },
  ];

  return (
    <nav className={styles.adminNav}>
      <div className={styles.container}>
        <Link href="/" className={styles.homeLink}>
          Home
        </Link>
        <div className={styles.tabs}>
          {tabs.map((tab) => (
            <Link
              key={tab.path}
              href={tab.path}
              className={`${styles.tab} ${
                pathname === tab.path ? styles.active : ""
              }`}
            >
              {tab.name}
            </Link>
          ))}
        </div>
      </div>
    </nav>
  );
}
