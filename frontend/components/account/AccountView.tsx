"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/context/AuthContext";
import styles from "@/styles/orders.module.css";

export default function AccountView() {
  const { user, isAuthenticated, isLoading, logout } = useAuth();
  const router = useRouter();

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      router.replace("/login");
    }
  }, [isAuthenticated, isLoading, router]);

  if (isLoading || !isAuthenticated || !user) {
    return <main className={styles.pageState}>Loading your account...</main>;
  }

  const handleLogout = () => {
    logout();
    router.push("/login");
  };

  return (
    <main className={styles.page}>
      <section className={styles.pageHeading}>
        <div>
          <p className={styles.eyebrow}>Account</p>
          <h1>Your basic information</h1>
          <p className={styles.lead}>Review the profile details currently available in your signed-in account.</p>
        </div>
      </section>

      <section className={styles.accountCard}>
        <div className={styles.cardHeader}>
          <h2>Profile</h2>
          <span className={styles.statusPillMuted}>{user.role}</span>
        </div>

        <div className={styles.accountGrid}>
          <div className={styles.infoPanel}>
            <h3>Full name</h3>
            <p>{user.name}</p>
          </div>

          <div className={styles.infoPanel}>
            <h3>Email address</h3>
            <p>{user.email}</p>
          </div>

          <div className={styles.infoPanel}>
            <h3>User ID</h3>
            <p>{user.userId}</p>
          </div>

          <div className={styles.infoPanel}>
            <h3>Member since</h3>
            <p>{new Date(user.createdAt).toLocaleString()}</p>
          </div>
        </div>

        <div className={styles.accountActions}>
          <button type="button" className={styles.secondaryButton} onClick={handleLogout}>
            Logout
          </button>
        </div>
      </section>
    </main>
  );
}
