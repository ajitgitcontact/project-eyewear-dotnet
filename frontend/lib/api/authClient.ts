import { LoginRequest, LoginResponse } from "@/lib/types";

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5047/api";

export class AuthClient {
  static async login(credentials: LoginRequest): Promise<LoginResponse> {
    try {
      const url = `${API_BASE_URL}/Users/login`;
      console.log("🔐 Login attempt to:", url);
      
      const response = await fetch(url, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        credentials: "include",
        body: JSON.stringify(credentials),
      });

      console.log("📡 Response status:", response.status);

      if (!response.ok) {
        const error = await response.json().catch(() => ({ message: `HTTP ${response.status}` }));
        console.error("❌ Login error:", error);
        throw new Error(error.message || "Authentication failed");
      }

      const data = await response.json();
      console.log("✅ Login successful:", data);
      return data;
    } catch (err) {
      console.error("🔥 Fetch error:", err);
      throw err;
    }
  }

  static setToken(token: string): void {
    if (typeof window !== "undefined") {
      localStorage.setItem("authToken", token);
    }
  }

  static getToken(): string | null {
    if (typeof window !== "undefined") {
      return localStorage.getItem("authToken");
    }
    return null;
  }

  static clearToken(): void {
    if (typeof window !== "undefined") {
      localStorage.removeItem("authToken");
      localStorage.removeItem("user");
    }
  }

  static setUser(user: any): void {
    if (typeof window !== "undefined") {
      localStorage.setItem("user", JSON.stringify(user));
    }
  }

  static getUser(): any {
    if (typeof window !== "undefined") {
      const user = localStorage.getItem("user");
      return user ? JSON.parse(user) : null;
    }
    return null;
  }
}
