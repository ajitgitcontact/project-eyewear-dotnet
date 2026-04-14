"use client";

import { usePathname, useRouter } from "next/navigation";
import styles from "@/styles/adminnav.module.css";

export default function AdminNav() {
  const pathname = usePathname();
  const router = useRouter();

  const tabs = [
    { name: "User Management", path: "/admin/users" },
    { name: "Product Management", path: "/admin/products" },
  ];

  return (
    <nav className={styles.adminNav}>
      <div className={styles.container}>
        <div className={styles.tabs}>
          {tabs.map((tab) => (
            <button
              key={tab.path}
              onClick={() => router.push(tab.path)}
              className={`${styles.tab} ${
                pathname === tab.path ? styles.active : ""
              }`}
            >
              {tab.name}
            </button>
          ))}
        </div>
      </div>
    </nav>
  );
}
