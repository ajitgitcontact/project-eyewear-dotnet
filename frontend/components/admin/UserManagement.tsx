"use client";

import { useState, useEffect } from "react";
import { UserManagementClient, UserListResponse, CreateUserRequest, UpdateUserRequest } from "@/lib/api/userClient";
import styles from "@/styles/usermanagement.module.css";

interface FormState {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  role: "ADMIN" | "SUPER_ADMIN" | "CUSTOMER";
}

export default function UserManagement() {
  const [users, setUsers] = useState<UserListResponse[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  
  const [editingId, setEditingId] = useState<number | null>(null);
  const [formData, setFormData] = useState<FormState>({
    firstName: "",
    lastName: "",
    email: "",
    password: "",
    role: "CUSTOMER",
  });

  // Fetch users on component mount
  useEffect(() => {
    fetchUsers();
  }, []);

  const fetchUsers = async () => {
    try {
      setLoading(true);
      const data = await UserManagementClient.getAllUsers();
      setUsers(data);
      setError(null);
    } catch (err) {
      const msg = err instanceof Error ? err.message : "Failed to load users";
      setError(msg);
      console.error("❌ Error fetching users:", err);
    } finally {
      setLoading(false);
    }
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setSuccess(null);

    try {
      if (editingId) {
        // Update user
        const updateData: UpdateUserRequest = {
          name: `${formData.firstName} ${formData.lastName}`.trim(),
          email: formData.email,
          role: formData.role,
        };
        await UserManagementClient.updateUser(editingId, updateData);
        setSuccess("User updated successfully!");
      } else {
        // Create user
        const createData: CreateUserRequest = {
          firstName: formData.firstName,
          lastName: formData.lastName,
          email: formData.email,
          password: formData.password,
          role: formData.role,
        };
        await UserManagementClient.createUser(createData);
        setSuccess("User created successfully!");
      }

      // Reset form and reload users
      resetForm();
      await fetchUsers();
    } catch (err) {
      const msg = err instanceof Error ? err.message : "Operation failed";
      setError(msg);
      console.error("❌ Error:", err);
    }
  };

  const handleEdit = (user: UserListResponse) => {
    setEditingId(user.userId);
    // Split the full name into first and last name
    const nameParts = user.name.split(' ');
    const firstName = nameParts[0] || '';
    const lastName = nameParts.slice(1).join(' ') || '';

    setFormData({
      firstName: firstName,
      lastName: lastName,
      email: user.email,
      password: "",
      role: user.role,
    });
  };

  const handleDelete = async (userId: number) => {
    if (!confirm("Are you sure you want to delete this user?")) return;

    try {
      setError(null);
      await UserManagementClient.deleteUser(userId);
      setSuccess("User deleted successfully!");
      await fetchUsers();
    } catch (err) {
      const msg = err instanceof Error ? err.message : "Failed to delete user";
      setError(msg);
    }
  };

  const getRoleClass = (role: string) => {
    switch (role) {
      case "ADMIN":
        return styles.admin;
      case "SUPER_ADMIN":
        return styles.super_admin;
      case "CUSTOMER":
        return styles.customer;
      default:
        return styles.customer;
    }
  };

  const resetForm = () => {
    setEditingId(null);
    setFormData({
      firstName: "",
      lastName: "",
      email: "",
      password: "",
      role: "CUSTOMER",
    });
  };

  return (
    <div className={styles.container}>
      <div className={styles.content}>
        {/* Form Section */}
        <div className={styles.formSection}>
          <h2>{editingId ? "Edit User" : "Add New User"}</h2>

          {error && <div className={styles.alert + " " + styles.error}>{error}</div>}
          {success && <div className={styles.alert + " " + styles.success}>{success}</div>}

          <form onSubmit={handleSubmit} className={styles.form}>
            <div className={styles.formGroup}>
              <label htmlFor="firstName">First Name</label>
              <input
                id="firstName"
                type="text"
                name="firstName"
                value={formData.firstName}
                onChange={handleInputChange}
                placeholder="John"
                required
              />
            </div>

            <div className={styles.formGroup}>
              <label htmlFor="lastName">Last Name</label>
              <input
                id="lastName"
                type="text"
                name="lastName"
                value={formData.lastName}
                onChange={handleInputChange}
                placeholder="Doe"
                required
              />
            </div>

            <div className={styles.formGroup}>
              <label htmlFor="email">Email</label>
              <input
                id="email"
                type="email"
                name="email"
                value={formData.email}
                onChange={handleInputChange}
                placeholder="john@example.com"
                required
              />
            </div>

            {!editingId && (
              <div className={styles.formGroup}>
                <label htmlFor="password">Password</label>
                <input
                  id="password"
                  type="password"
                  name="password"
                  value={formData.password}
                  onChange={handleInputChange}
                  placeholder="••••••••"
                  required
                />
              </div>
            )}

            <div className={styles.formGroup}>
              <label htmlFor="role">Role</label>
              <select
                id="role"
                name="role"
                value={formData.role}
                onChange={handleInputChange}
              >
                <option value="CUSTOMER">Customer</option>
                <option value="ADMIN">Admin</option>
                <option value="SUPER_ADMIN">Super Admin</option>
              </select>
            </div>

            <div className={styles.formActions}>
              <button type="submit" className={styles.submitBtn}>
                {editingId ? "Update User" : "Add User"}
              </button>
              {editingId && (
                <button
                  type="button"
                  onClick={resetForm}
                  className={styles.cancelBtn}
                >
                  Cancel
                </button>
              )}
            </div>
          </form>
        </div>

        {/* Users List Section */}
        <div className={styles.listSection}>
          <h2>Users List ({users.length})</h2>

          {loading ? (
            <p className={styles.loading}>Loading users...</p>
          ) : users.length === 0 ? (
            <p className={styles.empty}>No users found.</p>
          ) : (
            <div className={styles.tableWrapper}>
              <table className={styles.table}>
                <thead>
                  <tr>
                    <th>ID</th>
                    <th>Name</th>
                    <th>Email</th>
                    <th>Role</th>
                    <th>Created</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {users.map((user, index) => (
                    <tr key={user.userId || `user-${index}`}>
                      <td>{user.userId}</td>
                      <td>{user.name}</td>
                      <td>{user.email}</td>
                      <td>
                        <span className={`${styles.role} ${getRoleClass(user.role)}`}>
                          {user.role}
                        </span>
                      </td>
                      <td>{new Date(user.createdAt).toLocaleDateString()}</td>
                      <td className={styles.actions}>
                        <button
                          onClick={() => handleEdit(user)}
                          className={styles.editBtn}
                        >
                          Edit
                        </button>
                        <button
                          onClick={() => handleDelete(user.userId)}
                          className={styles.deleteBtn}
                        >
                          Delete
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
