"use client";

import { useEffect, useState } from "react";
import { useAuth } from "@/context/AuthContext";
import {
  ProductManagementClientInstance,
  CreateProductRequest,
  ProductListResponse,
} from "@/lib/api/productManagementClient";
import styles from "@/styles/productmanagement.module.css";

export default function ProductManagement() {
  const { user, isAuthenticated } = useAuth();
  const [products, setProducts] = useState<ProductListResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [editingId, setEditingId] = useState<number | null>(null);

  const [formData, setFormData] = useState<CreateProductRequest>({
    sku: "",
    name: "",
    description: "",
    brand: "",
    category: "Sunglasses",
    basePrice: 0,
    availableQuantity: 0,
  });

  const categories = [
    "Sunglasses",
    "Anti Blue Lens",
    "Clearance",
    "Coming Soon",
    "Accessories",
  ];

  // Fetch all products
  const fetchProducts = async () => {
    try {
      setLoading(true);
      const data = await ProductManagementClientInstance.getAllProducts();
      setProducts(data);
      setError(null);
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "Failed to fetch products"
      );
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchProducts();
  }, []);

  // Handle form input changes
  const handleInputChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>
  ) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: name === "basePrice" || name === "availableQuantity" ? parseFloat(value) || 0 : value,
    }));
  };

  // Handle form submission
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      if (editingId) {
        await ProductManagementClientInstance.updateProduct(editingId, formData);
      } else {
        await ProductManagementClientInstance.createProduct(formData);
      }

      // Reset form and refresh products
      setFormData({
        sku: "",
        name: "",
        description: "",
        brand: "",
        category: "Sunglasses",
        basePrice: 0,
        availableQuantity: 0,
      });
      setEditingId(null);
      await fetchProducts();
      setError(null);
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to save product"
      );
    }
  };

  // Handle edit
  const handleEdit = (product: ProductListResponse) => {
    setFormData({
      sku: product.sku,
      name: product.name,
      description: product.description || "",
      brand: product.brand || "",
      category: product.category,
      basePrice: product.basePrice,
      availableQuantity: product.availableQuantity,
    });
    setEditingId(product.productId);
  };

  // Handle delete
  const handleDelete = async (id: number) => {
    if (!confirm("Are you sure you want to delete this product?")) {
      return;
    }

    try {
      await ProductManagementClientInstance.deleteProduct(id);
      await fetchProducts();
      setError(null);
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to delete product"
      );
    }
  };

  // Handle cancel edit
  const handleCancel = () => {
    setFormData({
      sku: "",
      name: "",
      description: "",
      brand: "",
      category: "Sunglasses",
      basePrice: 0,
      availableQuantity: 0,
    });
    setEditingId(null);
  };

  if (!isAuthenticated || (user?.role !== "ADMIN" && user?.role !== "SUPER_ADMIN")) {
    return (
      <div className={styles.unauthorized}>
        <h2>Access Denied</h2>
        <p>You don't have permission to access this page.</p>
      </div>
    );
  }

  return (
    <div className={styles.container}>
      <h1 className={styles.title}>Product Management</h1>

      {error && <div className={styles.error}>{error}</div>}

      <div className={styles.content}>
        {/* Product Form */}
        <div className={styles.formSection}>
          <h2>{editingId ? "Edit Product" : "Add New Product"}</h2>
          <form onSubmit={handleSubmit} className={styles.form}>
            <div className={styles.formGroup}>
              <label htmlFor="sku">SKU *</label>
              <input
                type="text"
                id="sku"
                name="sku"
                value={formData.sku}
                onChange={handleInputChange}
                required
                placeholder="e.g., SKU001"
              />
            </div>

            <div className={styles.formGroup}>
              <label htmlFor="name">Product Name *</label>
              <input
                type="text"
                id="name"
                name="name"
                value={formData.name}
                onChange={handleInputChange}
                required
                placeholder="Enter product name"
              />
            </div>

            <div className={styles.formGroup}>
              <label htmlFor="brand">Brand</label>
              <input
                type="text"
                id="brand"
                name="brand"
                value={formData.brand}
                onChange={handleInputChange}
                placeholder="Enter brand name"
              />
            </div>

            <div className={styles.formGroup}>
              <label htmlFor="category">Category *</label>
              <select
                id="category"
                name="category"
                value={formData.category}
                onChange={handleInputChange}
                required
              >
                {categories.map((cat) => (
                  <option key={cat} value={cat}>
                    {cat}
                  </option>
                ))}
              </select>
            </div>

            <div className={styles.formGroup}>
              <label htmlFor="basePrice">Base Price *</label>
              <input
                type="number"
                id="basePrice"
                name="basePrice"
                value={formData.basePrice}
                onChange={handleInputChange}
                required
                step="0.01"
                min="0"
                placeholder="0.00"
              />
            </div>

            <div className={styles.formGroup}>
              <label htmlFor="availableQuantity">Available Quantity *</label>
              <input
                type="number"
                id="availableQuantity"
                name="availableQuantity"
                value={formData.availableQuantity}
                onChange={handleInputChange}
                required
                min="0"
                placeholder="0"
              />
            </div>

            <div className={styles.formGroup}>
              <label htmlFor="description">Description</label>
              <textarea
                id="description"
                name="description"
                value={formData.description}
                onChange={handleInputChange}
                placeholder="Enter product description"
                rows={4}
              />
            </div>

            <div className={styles.formButtons}>
              <button type="submit" className={styles.submitBtn}>
                {editingId ? "Update Product" : "Add Product"}
              </button>
              {editingId && (
                <button
                  type="button"
                  onClick={handleCancel}
                  className={styles.cancelBtn}
                >
                  Cancel
                </button>
              )}
            </div>
          </form>
        </div>

        {/* Products Table */}
        <div className={styles.tableSection}>
          <h2>Products List</h2>
          {loading ? (
            <div className={styles.loading}>Loading products...</div>
          ) : products.length === 0 ? (
            <div className={styles.empty}>No products found</div>
          ) : (
            <div className={styles.tableWrapper}>
              <table className={styles.table}>
                <thead>
                  <tr>
                    <th>SKU</th>
                    <th>Name</th>
                    <th>Category</th>
                    <th>Price</th>
                    <th>Quantity</th>
                    <th>Sold</th>
                    <th>Status</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {products.map((product) => (
                    <tr key={product.productId}>
                      <td
                        className={styles.sku}
                        title={product.sku}
                      >
                        {product.sku}
                      </td>
                      <td className={styles.name}>{product.name}</td>
                      <td>{product.category}</td>
                      <td className={styles.price}>
                        ${product.basePrice.toFixed(2)}
                      </td>
                      <td className={styles.quantity}>
                        {product.availableQuantity}
                      </td>
                      <td>{product.soldQuantity}</td>
                      <td>
                        <span
                          className={`${styles.badge} ${
                            product.isActive ? styles.active : styles.inactive
                          }`}
                        >
                          {product.isActive ? "Active" : "Inactive"}
                        </span>
                      </td>
                      <td className={styles.actions}>
                        <button
                          onClick={() => handleEdit(product)}
                          className={styles.editBtn}
                        >
                          Edit
                        </button>
                        <button
                          onClick={() => handleDelete(product.productId)}
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
