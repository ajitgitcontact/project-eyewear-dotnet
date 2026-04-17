"use client";

import { useEffect, useState } from "react";
import { useAuth } from "@/context/AuthContext";
import {
  CreateFullProductRequest,
  CreateCustomizationImageRequest,
  CreateCustomizationOptionRequest,
  CreateCustomizationValueRequest,
  CreateProductImageRequest,
  ProductManagementClientInstance,
  ProductListResponse,
  UpdateProductRequest,
} from "@/lib/api/productManagementClient";
import styles from "@/styles/productmanagement.module.css";

function createEmptyProductImage(displayOrder: number): CreateProductImageRequest {
  return {
    imageUrl: "",
    isPrimary: displayOrder === 1,
    displayOrder,
  };
}

function createEmptyCustomizationImage(): CreateCustomizationImageRequest {
  return {
    imageUrl: "",
  };
}

function createEmptyCustomizationValue(): CreateCustomizationValueRequest {
  return {
    value: "",
    additionalPrice: 0,
    customizationImages: [createEmptyCustomizationImage()],
  };
}

function createEmptyCustomizationOption(displayOrder: number): CreateCustomizationOptionRequest {
  return {
    name: "",
    isRequired: true,
    displayOrder,
    values: [createEmptyCustomizationValue()],
  };
}

function createInitialFormData(): CreateFullProductRequest {
  return {
    sku: "",
    name: "",
    description: "",
    brand: "",
    category: "Sunglasses",
    basePrice: 0,
    availableQuantity: 0,
    soldQuantity: 0,
    priority: 0,
    hasPrescription: false,
    images: [createEmptyProductImage(1)],
    customizationOptions: [createEmptyCustomizationOption(1)],
  };
}

export default function ProductManagement() {
  const { user, isAuthenticated } = useAuth();
  const [products, setProducts] = useState<ProductListResponse[]>([]);
  const [searchTerm, setSearchTerm] = useState("");
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [formData, setFormData] = useState<CreateFullProductRequest>(createInitialFormData);

  const categories = [
    "Sunglasses",
    "Anti Blue Lens",
    "Clearance",
    "Coming Soon",
    "Accessories",
  ];

  const fetchProducts = async () => {
    try {
      setLoading(true);
      const data = await ProductManagementClientInstance.getAllProducts();
      setProducts(data);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to fetch products");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchProducts();
  }, []);

  const filteredProducts = products.filter((product) => {
    const query = searchTerm.trim().toLowerCase();
    if (!query) {
      return true;
    }

    return (
      product.name.toLowerCase().includes(query) ||
      product.sku.toLowerCase().includes(query) ||
      product.category.toLowerCase().includes(query) ||
      (product.brand ?? "").toLowerCase().includes(query)
    );
  });

  const resetForm = () => {
    setFormData(createInitialFormData());
    setEditingId(null);
  };

  const getNewProductNumberValue = (value: number) => {
    if (editingId) {
      return value;
    }

    return value === 0 ? "" : value;
  };

  const updateField = <K extends keyof CreateFullProductRequest>(
    field: K,
    value: CreateFullProductRequest[K]
  ) => {
    setFormData((prev) => ({
      ...prev,
      [field]: value,
    }));
  };

  const handleInputChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>
  ) => {
    const { name, value, type } = e.target;

    if (type === "checkbox" && "checked" in e.target) {
      updateField(name as keyof CreateFullProductRequest, e.target.checked as never);
      return;
    }

    if (["basePrice", "availableQuantity", "soldQuantity", "priority"].includes(name)) {
      updateField(name as keyof CreateFullProductRequest, (Number(value) || 0) as never);
      return;
    }

    updateField(name as keyof CreateFullProductRequest, value as never);
  };

  const updateProductImage = (
    imageIndex: number,
    field: keyof CreateProductImageRequest,
    value: string | number | boolean
  ) => {
    setFormData((prev) => ({
      ...prev,
      images: prev.images.map((image, index) =>
        index === imageIndex ? { ...image, [field]: value } : image
      ),
    }));
  };

  const addProductImage = () => {
    setFormData((prev) => ({
      ...prev,
      images: [...prev.images, createEmptyProductImage(prev.images.length + 1)],
    }));
  };

  const removeProductImage = (imageIndex: number) => {
    setFormData((prev) => {
      const nextImages = prev.images
        .filter((_, index) => index !== imageIndex)
        .map((image, index) => ({
          ...image,
          displayOrder: index + 1,
          isPrimary: image.isPrimary,
        }));

      if (nextImages.length > 0 && !nextImages.some((image) => image.isPrimary)) {
        nextImages[0].isPrimary = true;
      }

      return {
        ...prev,
        images: nextImages.length > 0 ? nextImages : [createEmptyProductImage(1)],
      };
    });
  };

  const updateOption = (
    optionIndex: number,
    field: keyof CreateCustomizationOptionRequest,
    value: string | number | boolean
  ) => {
    setFormData((prev) => ({
      ...prev,
      customizationOptions: prev.customizationOptions.map((option, index) =>
        index === optionIndex ? { ...option, [field]: value } : option
      ),
    }));
  };

  const addOption = () => {
    setFormData((prev) => ({
      ...prev,
      customizationOptions: [
        ...prev.customizationOptions,
        createEmptyCustomizationOption(prev.customizationOptions.length + 1),
      ],
    }));
  };

  const removeOption = (optionIndex: number) => {
    setFormData((prev) => {
      const nextOptions = prev.customizationOptions
        .filter((_, index) => index !== optionIndex)
        .map((option, index) => ({
          ...option,
          displayOrder: index + 1,
        }));

      return {
        ...prev,
        customizationOptions: nextOptions.length > 0 ? nextOptions : [createEmptyCustomizationOption(1)],
      };
    });
  };

  const updateValue = (
    optionIndex: number,
    valueIndex: number,
    field: keyof CreateCustomizationValueRequest,
    value: string | number
  ) => {
    setFormData((prev) => ({
      ...prev,
      customizationOptions: prev.customizationOptions.map((option, currentOptionIndex) =>
        currentOptionIndex === optionIndex
          ? {
              ...option,
              values: option.values.map((optionValue, currentValueIndex) =>
                currentValueIndex === valueIndex ? { ...optionValue, [field]: value } : optionValue
              ),
            }
          : option
      ),
    }));
  };

  const addValue = (optionIndex: number) => {
    setFormData((prev) => ({
      ...prev,
      customizationOptions: prev.customizationOptions.map((option, index) =>
        index === optionIndex
          ? {
              ...option,
              values: [...option.values, createEmptyCustomizationValue()],
            }
          : option
      ),
    }));
  };

  const removeValue = (optionIndex: number, valueIndex: number) => {
    setFormData((prev) => ({
      ...prev,
      customizationOptions: prev.customizationOptions.map((option, index) => {
        if (index !== optionIndex) {
          return option;
        }

        const nextValues = option.values.filter((_, currentValueIndex) => currentValueIndex !== valueIndex);
        return {
          ...option,
          values: nextValues.length > 0 ? nextValues : [createEmptyCustomizationValue()],
        };
      }),
    }));
  };

  const updateCustomizationImage = (
    optionIndex: number,
    valueIndex: number,
    imageIndex: number,
    imageUrl: string
  ) => {
    setFormData((prev) => ({
      ...prev,
      customizationOptions: prev.customizationOptions.map((option, currentOptionIndex) =>
        currentOptionIndex === optionIndex
          ? {
              ...option,
              values: option.values.map((optionValue, currentValueIndex) =>
                currentValueIndex === valueIndex
                  ? {
                      ...optionValue,
                      customizationImages: optionValue.customizationImages.map((image, currentImageIndex) =>
                        currentImageIndex === imageIndex ? { ...image, imageUrl } : image
                      ),
                    }
                  : optionValue
              ),
            }
          : option
      ),
    }));
  };

  const addCustomizationImage = (optionIndex: number, valueIndex: number) => {
    setFormData((prev) => ({
      ...prev,
      customizationOptions: prev.customizationOptions.map((option, currentOptionIndex) =>
        currentOptionIndex === optionIndex
          ? {
              ...option,
              values: option.values.map((optionValue, currentValueIndex) =>
                currentValueIndex === valueIndex
                  ? {
                      ...optionValue,
                      customizationImages: [
                        ...optionValue.customizationImages,
                        createEmptyCustomizationImage(),
                      ],
                    }
                  : optionValue
              ),
            }
          : option
      ),
    }));
  };

  const removeCustomizationImage = (
    optionIndex: number,
    valueIndex: number,
    imageIndex: number
  ) => {
    setFormData((prev) => ({
      ...prev,
      customizationOptions: prev.customizationOptions.map((option, currentOptionIndex) =>
        currentOptionIndex === optionIndex
          ? {
              ...option,
              values: option.values.map((optionValue, currentValueIndex) => {
                if (currentValueIndex !== valueIndex) {
                  return optionValue;
                }

                const nextImages = optionValue.customizationImages.filter(
                  (_, currentImageIndex) => currentImageIndex !== imageIndex
                );

                return {
                  ...optionValue,
                  customizationImages: nextImages.length > 0 ? nextImages : [createEmptyCustomizationImage()],
                };
              }),
            }
          : option
      ),
    }));
  };

  const validateFullProduct = () => {
    if (!formData.images.some((image) => image.imageUrl.trim())) {
      return "Add at least one product image URL.";
    }

    if (!formData.images.some((image) => image.isPrimary)) {
      return "Select one primary product image.";
    }

    for (const [optionIndex, option] of formData.customizationOptions.entries()) {
      if (!option.name.trim()) {
        return `Customization option ${optionIndex + 1} needs a name.`;
      }

      if (option.values.length === 0) {
        return `Customization option ${option.name || optionIndex + 1} needs at least one value.`;
      }

      for (const [valueIndex, optionValue] of option.values.entries()) {
        if (!optionValue.value.trim()) {
          return `Customization value ${valueIndex + 1} in ${option.name || "option"} needs a name.`;
        }
      }
    }

    return null;
  };

  const buildUpdatePayload = (): UpdateProductRequest => ({
    sku: formData.sku,
    name: formData.name,
    description: formData.description || "",
    brand: formData.brand || "",
    category: formData.category,
    basePrice: formData.basePrice,
    availableQuantity: formData.availableQuantity,
    priority: formData.priority,
    isActive: true,
  });

  const sanitizeCreatePayload = (): CreateFullProductRequest => ({
    ...formData,
    description: formData.description?.trim() || "",
    brand: formData.brand?.trim() || "",
    images: formData.images
      .filter((image) => image.imageUrl.trim())
      .map((image, index) => ({
        ...image,
        imageUrl: image.imageUrl.trim(),
        displayOrder: index + 1,
      })),
    customizationOptions: formData.customizationOptions.map((option, optionIndex) => ({
      ...option,
      name: option.name.trim(),
      displayOrder: optionIndex + 1,
      values: option.values.map((optionValue) => ({
        ...optionValue,
        value: optionValue.value.trim(),
        customizationImages: optionValue.customizationImages
          .filter((image) => image.imageUrl.trim())
          .map((image) => ({
            imageUrl: image.imageUrl.trim(),
          })),
      })),
    })),
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setSuccess(null);

    try {
      setSaving(true);

      if (editingId) {
        await ProductManagementClientInstance.updateProduct(editingId, buildUpdatePayload());
        setSuccess("Product updated successfully.");
      } else {
        const validationError = validateFullProduct();
        if (validationError) {
          setError(validationError);
          return;
        }

        await ProductManagementClientInstance.createProduct(sanitizeCreatePayload());
        setSuccess("Full product created successfully with customizations.");
      }

      resetForm();
      await fetchProducts();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to save product");
    } finally {
      setSaving(false);
    }
  };

  const handleEdit = (product: ProductListResponse) => {
    setFormData({
      sku: product.sku,
      name: product.name,
      description: product.description || "",
      brand: product.brand || "",
      category: product.category,
      basePrice: product.basePrice,
      availableQuantity: product.availableQuantity,
      soldQuantity: product.soldQuantity,
      priority: product.priority,
      hasPrescription: product.hasPrescription,
      images:
        product.images.length > 0
          ? product.images.map((image, index) => ({
              imageUrl: image.imageUrl,
              isPrimary: image.isPrimary,
              displayOrder: index + 1,
            }))
          : [createEmptyProductImage(1)],
      customizationOptions:
        product.customizationOptions.length > 0
          ? product.customizationOptions.map((option, optionIndex) => ({
              name: option.name,
              isRequired: option.isRequired,
              displayOrder: option.displayOrder || optionIndex + 1,
              values:
                option.values.length > 0
                  ? option.values.map((optionValue) => ({
                      value: optionValue.value,
                      additionalPrice: optionValue.additionalPrice,
                      customizationImages:
                        optionValue.customizationImages.length > 0
                          ? optionValue.customizationImages.map((image) => ({
                              imageUrl: image.imageUrl,
                            }))
                          : [createEmptyCustomizationImage()],
                    }))
                  : [createEmptyCustomizationValue()],
            }))
          : [createEmptyCustomizationOption(1)],
    });
    setEditingId(product.productId);
    setSuccess("Editing basic product details. Nested customization editing is not saved yet.");
  };

  const handleDelete = async (id: number) => {
    if (!confirm("Are you sure you want to delete this product?")) {
      return;
    }

    try {
      await ProductManagementClientInstance.deleteProduct(id);
      await fetchProducts();
      setError(null);
      setSuccess("Product deleted successfully.");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to delete product");
    }
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
      {success && <div className={styles.success}>{success}</div>}

      <div className={styles.content}>
        <div className={styles.formSection}>
          <h2>{editingId ? "Edit Product Basics" : "Add Full Product"}</h2>
          <form onSubmit={handleSubmit} className={styles.form}>
            <div className={styles.twoColumnGrid}>
              <div className={styles.formGroup}>
                <label htmlFor="sku">SKU *</label>
                <input id="sku" name="sku" value={formData.sku} onChange={handleInputChange} required />
              </div>

              <div className={styles.formGroup}>
                <label htmlFor="name">Product Name *</label>
                <input id="name" name="name" value={formData.name} onChange={handleInputChange} required />
              </div>

              <div className={styles.formGroup}>
                <label htmlFor="brand">Brand</label>
                <input id="brand" name="brand" value={formData.brand} onChange={handleInputChange} />
              </div>

              <div className={styles.formGroup}>
                <label htmlFor="category">Category *</label>
                <select id="category" name="category" value={formData.category} onChange={handleInputChange} required>
                  {categories.map((category) => (
                    <option key={category} value={category}>
                      {category}
                    </option>
                  ))}
                </select>
              </div>

              <div className={styles.formGroup}>
                <label htmlFor="basePrice">Base Price *</label>
                <input
                  id="basePrice"
                  type="number"
                  name="basePrice"
                  value={getNewProductNumberValue(formData.basePrice)}
                  onChange={handleInputChange}
                  step="0.01"
                  min="0"
                  placeholder="Enter base price"
                  required
                />
              </div>

              <div className={styles.formGroup}>
                <label htmlFor="availableQuantity">Available Quantity *</label>
                <input
                  id="availableQuantity"
                  type="number"
                  name="availableQuantity"
                  value={getNewProductNumberValue(formData.availableQuantity)}
                  onChange={handleInputChange}
                  min="0"
                  placeholder="Enter quantity"
                  required
                />
              </div>

              <div className={styles.formGroup}>
                <label htmlFor="soldQuantity">Sold Quantity</label>
                <input
                  id="soldQuantity"
                  type="number"
                  name="soldQuantity"
                  value={formData.soldQuantity}
                  onChange={handleInputChange}
                  min="0"
                />
              </div>

              <div className={styles.formGroup}>
                <label htmlFor="priority">Priority</label>
                <input
                  id="priority"
                  type="number"
                  name="priority"
                  value={formData.priority}
                  onChange={handleInputChange}
                  min="0"
                />
              </div>
            </div>

            <div className={styles.formGroup}>
              <label htmlFor="description">Description</label>
              <textarea
                id="description"
                name="description"
                value={formData.description}
                onChange={handleInputChange}
                rows={4}
              />
            </div>

            <label className={styles.checkboxRow}>
              <input
                type="checkbox"
                name="hasPrescription"
                checked={formData.hasPrescription}
                onChange={handleInputChange}
              />
              <span>Supports prescription lenses</span>
            </label>

            <section className={styles.editorSection}>
              <div className={styles.sectionHeader}>
                <h3>Product Images</h3>
                <button type="button" className={styles.secondaryBtn} onClick={addProductImage}>
                  Add Image
                </button>
              </div>

              {formData.images.map((image, imageIndex) => (
                <div className={styles.card} key={`product-image-${imageIndex}`}>
                  <div className={styles.inlineActions}>
                    <strong>Image {imageIndex + 1}</strong>
                    <button
                      type="button"
                      className={styles.textBtn}
                      onClick={() => removeProductImage(imageIndex)}
                    >
                      Remove
                    </button>
                  </div>

                  <div className={styles.twoColumnGrid}>
                    <div className={styles.formGroup}>
                      <label>Image URL *</label>
                      <input
                        value={image.imageUrl}
                        onChange={(e) => updateProductImage(imageIndex, "imageUrl", e.target.value)}
                        placeholder="/images/products/frame-front.jpg"
                      />
                    </div>

                    <div className={styles.formGroup}>
                      <label>Display Order</label>
                      <input
                        type="number"
                        value={image.displayOrder}
                        onChange={(e) =>
                          updateProductImage(imageIndex, "displayOrder", Number(e.target.value) || 0)
                        }
                        min="1"
                      />
                    </div>
                  </div>

                  <label className={styles.checkboxRow}>
                    <input
                      type="checkbox"
                      checked={image.isPrimary}
                      onChange={(e) => {
                        const checked = e.target.checked;
                        setFormData((prev) => ({
                          ...prev,
                          images: prev.images.map((currentImage, currentIndex) => ({
                            ...currentImage,
                            isPrimary: currentIndex === imageIndex ? checked : checked ? false : currentImage.isPrimary,
                          })),
                        }));
                      }}
                    />
                    <span>Primary product image</span>
                  </label>
                </div>
              ))}
            </section>

            <section className={styles.editorSection}>
              <div className={styles.sectionHeader}>
                <h3>Customization Options</h3>
                <button type="button" className={styles.secondaryBtn} onClick={addOption}>
                  Add Option
                </button>
              </div>

              {formData.customizationOptions.map((option, optionIndex) => (
                <div className={styles.card} key={`option-${optionIndex}`}>
                  <div className={styles.inlineActions}>
                    <strong>Option {optionIndex + 1}</strong>
                    <button type="button" className={styles.textBtn} onClick={() => removeOption(optionIndex)}>
                      Remove
                    </button>
                  </div>

                  <div className={styles.twoColumnGrid}>
                    <div className={styles.formGroup}>
                      <label>Option Name *</label>
                      <input
                        value={option.name}
                        onChange={(e) => updateOption(optionIndex, "name", e.target.value)}
                        placeholder="Frame Color"
                      />
                    </div>

                    <div className={styles.formGroup}>
                      <label>Display Order</label>
                      <input
                        type="number"
                        value={option.displayOrder}
                        onChange={(e) => updateOption(optionIndex, "displayOrder", Number(e.target.value) || 0)}
                        min="1"
                      />
                    </div>
                  </div>

                  <label className={styles.checkboxRow}>
                    <input
                      type="checkbox"
                      checked={option.isRequired}
                      onChange={(e) => updateOption(optionIndex, "isRequired", e.target.checked)}
                    />
                    <span>Required selection</span>
                  </label>

                  <div className={styles.subsection}>
                    <div className={styles.sectionHeader}>
                      <h4>Values</h4>
                      <button type="button" className={styles.secondaryBtn} onClick={() => addValue(optionIndex)}>
                        Add Value
                      </button>
                    </div>

                    {option.values.map((optionValue, valueIndex) => (
                      <div className={styles.nestedCard} key={`value-${optionIndex}-${valueIndex}`}>
                        <div className={styles.inlineActions}>
                          <strong>Value {valueIndex + 1}</strong>
                          <button
                            type="button"
                            className={styles.textBtn}
                            onClick={() => removeValue(optionIndex, valueIndex)}
                          >
                            Remove
                          </button>
                        </div>

                        <div className={styles.twoColumnGrid}>
                          <div className={styles.formGroup}>
                            <label>Value Name *</label>
                            <input
                              value={optionValue.value}
                              onChange={(e) => updateValue(optionIndex, valueIndex, "value", e.target.value)}
                              placeholder="Black"
                            />
                          </div>

                          <div className={styles.formGroup}>
                            <label>Additional Price</label>
                            <input
                              type="number"
                              value={optionValue.additionalPrice}
                              onChange={(e) =>
                                updateValue(
                                  optionIndex,
                                  valueIndex,
                                  "additionalPrice",
                                  Number(e.target.value) || 0
                                )
                              }
                              min="0"
                              step="0.01"
                            />
                          </div>
                        </div>

                        <div className={styles.subsection}>
                          <div className={styles.sectionHeader}>
                            <h4>Customization Images</h4>
                            <button
                              type="button"
                              className={styles.secondaryBtn}
                              onClick={() => addCustomizationImage(optionIndex, valueIndex)}
                            >
                              Add Image
                            </button>
                          </div>

                          {optionValue.customizationImages.map((image, imageIndex) => (
                            <div
                              className={styles.imageRow}
                              key={`customization-image-${optionIndex}-${valueIndex}-${imageIndex}`}
                            >
                              <input
                                value={image.imageUrl}
                                onChange={(e) =>
                                  updateCustomizationImage(
                                    optionIndex,
                                    valueIndex,
                                    imageIndex,
                                    e.target.value
                                  )
                                }
                                placeholder="/images/products/frame-black.jpg"
                              />
                              <button
                                type="button"
                                className={styles.textBtn}
                                onClick={() => removeCustomizationImage(optionIndex, valueIndex, imageIndex)}
                              >
                                Remove
                              </button>
                            </div>
                          ))}
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              ))}
            </section>

            <div className={styles.formButtons}>
              <button type="submit" className={styles.submitBtn} disabled={saving}>
                {saving ? "Saving..." : editingId ? "Update Product" : "Create Full Product"}
              </button>
              <button type="button" className={styles.cancelBtn} onClick={resetForm}>
                Reset Form
              </button>
            </div>
          </form>
        </div>

        <div className={styles.tableSection}>
          <div className={styles.tableHeader}>
            <h2>Products List</h2>
            <div className={styles.searchBox}>
              <input
                type="text"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                placeholder="Search by name, SKU, category, or brand"
              />
            </div>
          </div>
          {loading ? (
            <div className={styles.loading}>Loading products...</div>
          ) : filteredProducts.length === 0 ? (
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
                    <th>Qty</th>
                    <th>Options</th>
                    <th>Images</th>
                    <th>Status</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {filteredProducts.map((product) => (
                    <tr key={product.productId}>
                      <td className={styles.sku}>{product.sku}</td>
                      <td className={styles.name}>{product.name}</td>
                      <td>{product.category}</td>
                      <td className={styles.price}>${product.basePrice.toFixed(2)}</td>
                      <td className={styles.quantity}>{product.availableQuantity}</td>
                      <td>{product.customizationOptions.length}</td>
                      <td>{product.images.length}</td>
                      <td>
                        <span className={`${styles.badge} ${product.isActive ? styles.active : styles.inactive}`}>
                          {product.isActive ? "Active" : "Inactive"}
                        </span>
                      </td>
                      <td className={styles.actions}>
                        <button onClick={() => handleEdit(product)} className={styles.editBtn}>
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
