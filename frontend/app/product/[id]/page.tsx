export const dynamic = "force-dynamic";

import Link from "next/link";
import { Product } from "../../../lib/types";

interface PageProps {
  params: Promise<{
    id: string;
  }>;
}

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5047/api";

async function getProduct(id: string): Promise<Product> {
  try {
    const response = await fetch(`${API_BASE_URL}/Products/${id}`, {
      cache: "no-store",
    });

    if (!response.ok) {
      throw new Error(`Unable to fetch product details from backend. HTTP ${response.status}`);
    }

    return response.json();
  } catch (error) {
    const message = error instanceof Error ? error.message : "Unknown backend fetch error.";
    throw new Error(`Unable to fetch product details from backend. ${message}`);
  }
}

export default async function ProductPage({ params }: PageProps) {
  const { id } = await params;
  const product = await getProduct(id);
  const primaryImage = product.images.find((image) => image.isPrimary) ?? product.images[0];

  return (
    <main>
      <Link href="/" className="linkBack">
        Back to product list
      </Link>
      <section className="pageHeader">
        <div>
          <h1 className="title">{product.name}</h1>
          <p className="description">{product.description ?? "No description provided."}</p>
          <p className="productMeta">
            SKU {product.sku} | {product.category} | {product.brand ?? "No brand"}
          </p>
        </div>
      </section>

      <div className="detailGrid">
        <div className="detailSection">
          {primaryImage ? (
            <img className="cardImage" src={primaryImage.imageUrl} alt={product.name} />
          ) : (
            <div className="cardImage" style={{ display: "grid", placeItems: "center" }}>
              No image available
            </div>
          )}
          <div style={{ padding: "1.5rem" }}>
            <p className="price">Rs. {product.basePrice.toFixed(2)}</p>
            <p className="detailText">{product.availableQuantity} units available</p>
            <p className="detailText">{product.soldQuantity} sold | Priority {product.priority}</p>
            <p className="detailText">Prescription required: {product.hasPrescription ? "Yes" : "No"}</p>
            <p className="detailText">Product status: {product.isActive ? "Active" : "Inactive"}</p>
          </div>
        </div>

        <div className="detailSection">
          <div className="listGroup">
            <div>
              <h2 className="sectionHeading">Customization Options</h2>
              {product.customizationOptions.length === 0 ? (
                <p className="detailText">No customization options available.</p>
              ) : (
                product.customizationOptions.map((option) => (
                  <div key={option.customizationOptionId} className="optionBox">
                    <p className="label">
                      <strong>{option.name}</strong>
                      {option.isRequired ? "(Required)" : "(Optional)"}
                    </p>
                    {option.values.map((value) => (
                      <div key={value.customizationValueId} className="valueRow">
                        <span>{value.value}</span>
                        <span>+Rs. {value.additionalPrice.toFixed(2)}</span>
                      </div>
                    ))}
                  </div>
                ))
              )}
            </div>
            <div>
              <h2 className="sectionHeading">Product Images</h2>
              {product.images.length === 0 ? (
                <p className="detailText">No product images available.</p>
              ) : (
                product.images.map((image) => (
                  <div key={image.productImageId} style={{ marginBottom: "0.75rem" }}>
                    <img src={image.imageUrl} alt={`Product image ${image.productImageId}`} className="cardImage" />
                    <p className="detailText">{image.isPrimary ? "Primary image" : "Secondary image"}</p>
                  </div>
                ))
              )}
            </div>
          </div>
        </div>
      </div>
    </main>
  );
}
