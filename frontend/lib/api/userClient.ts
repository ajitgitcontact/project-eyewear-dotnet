import { User, LoginResponse } from "@/lib/types";

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5047/api";

export interface CreateUserRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  role: "ADMIN" | "SUPER_ADMIN" | "CUSTOMER";
}

export interface UpdateUserRequest {
  name: string;
  email: string;
  role: "ADMIN" | "SUPER_ADMIN" | "CUSTOMER";
}

export interface UserListResponse {
  userId: number;
  name: string;
  email: string;
  role: "ADMIN" | "SUPER_ADMIN" | "CUSTOMER";
  createdAt: string;
}

export class UserManagementClient {
  private static getHeaders() {
    const token = localStorage.getItem("authToken");
    return {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    };
  }

  static async createUser(data: CreateUserRequest): Promise<UserListResponse> {
    console.log("👤 Creating user...", data);

    const backendData = {
      firstName: data.firstName,
      lastName: data.lastName,
      email: data.email,
      password: data.password,
      userRole: data.role
    };

    console.log("📤 Sending to backend:", backendData);

    const response = await fetch(`${API_BASE_URL}/Users`, {
      method: "POST",
      headers: this.getHeaders(),
      body: JSON.stringify(backendData),
    });

    console.log("📡 Create user response:", {
      status: response.status,
      statusText: response.statusText,
      ok: response.ok,
      headers: Object.fromEntries(response.headers.entries())
    });

    if (!response.ok) {
      const errorText = await response.text();
      console.log("❌ Raw error response:", errorText);

      let errorMessage = `HTTP ${response.status}: ${response.statusText}`;
      try {
        const errorJson = JSON.parse(errorText);
        console.log("❌ Parsed error JSON:", errorJson);
        errorMessage = errorJson.message || errorMessage;
      } catch (parseError) {
        console.log("❌ Failed to parse error as JSON:", parseError);
        if (errorText) errorMessage = errorText;
      }

      console.error("❌ Create user failed:", { status: response.status, message: errorMessage });
      throw new Error(errorMessage);
    }

    const backendUser = await response.json();
    console.log("✅ User created successfully:", backendUser);

    // Transform backend response to frontend format
    const frontendUser: UserListResponse = {
      userId: backendUser.id,
      name: `${backendUser.firstName} ${backendUser.lastName}`.trim(),
      email: backendUser.email,
      role: backendUser.userRole,
      createdAt: backendUser.createdAt,
    };

    return frontendUser;
  }

  static async updateUser(userId: number, data: UpdateUserRequest): Promise<UserListResponse> {
    console.log("🔄 Updating user...", userId, data);
    const response = await fetch(`${API_BASE_URL}/Users/${userId}`, {
      method: "PUT",
      headers: this.getHeaders(),
      body: JSON.stringify(data),
    });

    if (!response.ok) {
      const errorText = await response.text();
      let errorMessage = `HTTP ${response.status}`;
      try {
        const errorJson = JSON.parse(errorText);
        errorMessage = errorJson.message || errorMessage;
      } catch {
        if (errorText) errorMessage = errorText;
      }
      console.error("❌ Update user failed:", { status: response.status, message: errorMessage });
      throw new Error(errorMessage);
    }

    const backendUser = await response.json();
    console.log("✅ User updated successfully:", backendUser);

    // Transform backend response to frontend format
    const frontendUser: UserListResponse = {
      userId: backendUser.id,
      name: `${backendUser.firstName} ${backendUser.lastName}`.trim(),
      email: backendUser.email,
      role: backendUser.userRole,
      createdAt: backendUser.createdAt,
    };

    return frontendUser;
  }

  static async getAllUsers(): Promise<UserListResponse[]> {
    console.log("📋 Fetching all users...");
    const response = await fetch(`${API_BASE_URL}/Users`, {
      method: "GET",
      headers: this.getHeaders(),
      cache: "no-store",
    });

    if (!response.ok) {
      const errorText = await response.text();
      let errorMessage = `HTTP ${response.status}`;
      try {
        const errorJson = JSON.parse(errorText);
        errorMessage = errorJson.message || errorMessage;
      } catch {
        if (errorText) errorMessage = errorText;
      }
      console.error("❌ Fetch users failed:", { status: response.status, message: errorMessage });
      throw new Error(errorMessage);
    }

    const backendUsers = await response.json();
    console.log("✅ Users fetched successfully:", backendUsers.length);

    // Transform backend response to frontend format
    const frontendUsers: UserListResponse[] = backendUsers.map((user: any) => ({
      userId: user.id,
      name: `${user.firstName} ${user.lastName}`.trim(),
      email: user.email,
      role: user.userRole,
      createdAt: user.createdAt,
    }));

    return frontendUsers;
  }

  static async getUserById(userId: number): Promise<UserListResponse> {
    console.log("🔍 Fetching user by ID...", userId);
    const response = await fetch(`${API_BASE_URL}/Users/${userId}`, {
      method: "GET",
      headers: this.getHeaders(),
      cache: "no-store",
    });

    if (!response.ok) {
      const errorText = await response.text();
      let errorMessage = `HTTP ${response.status}`;
      try {
        const errorJson = JSON.parse(errorText);
        errorMessage = errorJson.message || errorMessage;
      } catch {
        if (errorText) errorMessage = errorText;
      }
      console.error("❌ Fetch user failed:", { status: response.status, message: errorMessage });
      throw new Error(errorMessage);
    }

    const backendUser = await response.json();
    console.log("✅ User fetched successfully:", backendUser);

    // Transform backend response to frontend format
    const frontendUser: UserListResponse = {
      userId: backendUser.id,
      name: `${backendUser.firstName} ${backendUser.lastName}`.trim(),
      email: backendUser.email,
      role: backendUser.userRole,
      createdAt: backendUser.createdAt,
    };

    return frontendUser;
  }

  static async deleteUser(userId: number): Promise<void> {
    console.log("🗑️ Deleting user...", userId);
    const response = await fetch(`${API_BASE_URL}/Users/${userId}`, {
      method: "DELETE",
      headers: this.getHeaders(),
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: `HTTP ${response.status}` }));
      console.error("❌ Delete user failed:", error);
      throw new Error(error.message || "Failed to delete user");
    }

    console.log("✅ User deleted successfully:", userId);
  }
}
