"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/context/AuthContext";
import Header from "@/components/common/Header";
import AdminNav from "@/components/admin/AdminNav";
import UserManagement from "@/components/admin/UserManagement";

export default function AdminUsersPage() {
  const { user, isLoading } = useAuth();
  const router = useRouter();

  useEffect(() => {
    // Check if user is admin or super admin
    if (!isLoading && (!user || (user.role !== "ADMIN" && user.role !== "SUPER_ADMIN"))) {
      router.push("/");
    }
  }, [user, isLoading, router]);

  if (isLoading) {
    return (
      <div style={{ 
        minHeight: "100vh", 
        display: "flex", 
        alignItems: "center", 
        justifyContent: "center"
      }}>
        Loading...
      </div>
    );
  }

  // Only show if user is admin or super admin
  if (!user || (user.role !== "ADMIN" && user.role !== "SUPER_ADMIN")) {
    return null;
  }

  return (
    <>
      <Header />
      <AdminNav />
      <UserManagement />
    </>
  );
}
