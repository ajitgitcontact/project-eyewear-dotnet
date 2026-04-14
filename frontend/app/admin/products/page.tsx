"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/context/AuthContext";
import Header from "@/components/common/Header";
import AdminNav from "@/components/admin/AdminNav";
import ProductManagement from "@/components/admin/ProductManagement";

export default function AdminProductsPage() {
  const router = useRouter();
  const { user, isAuthenticated, isLoading } = useAuth();

  useEffect(() => {
    if (!isLoading && (!isAuthenticated || (user?.role !== "ADMIN" && user?.role !== "SUPER_ADMIN"))) {
      router.push("/login");
    }
  }, [isAuthenticated, user, isLoading, router]);

  if (isLoading) {
    return (
      <div style={{ padding: "40px 20px", textAlign: "center" }}>
        Loading...
      </div>
    );
  }

  if (!isAuthenticated || (user?.role !== "ADMIN" && user?.role !== "SUPER_ADMIN")) {
    return null;
  }

  return (
    <>
      <Header />
      <AdminNav />
      <ProductManagement />
    </>
  );
}
