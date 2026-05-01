export const dynamic = "force-dynamic";

import Link from "next/link";
import { Product } from "../../../lib/types";
import { formatUsd } from "@/lib/currency";
import { demoProductImageSrc } from "@/lib/demoProductImage";
import ProductActionPanel from "@/components/products/ProductActionPanel";

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
            {product.category} | {product.brand ?? "No brand"}
          </p>
        </div>
      </section>

      <div className="detailGrid">
        <div className="detailSection">
          <img className="cardImage" src={demoProductImageSrc} alt={product.name} />
          <div style={{ padding: "1.5rem" }}>
            <p className="price">{formatUsd(product.basePrice)}</p>
            {product.availableQuantity <= 0 ? <p className="detailText">Out of stock</p> : null}
            <p className="detailText">Prescription required: {product.hasPrescription ? "Yes" : "No"}</p>
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
                        <span>+{formatUsd(value.additionalPrice)}</span>
                      </div>
                    ))}
                  </div>
                ))
              )}
            </div>
            <div>
              <h2 className="sectionHeading">Product Images</h2>
              <div style={{ marginBottom: "0.75rem" }}>
                <img src={demoProductImageSrc} alt={product.name} className="cardImage" />
                <p className="detailText">Demo product image</p>
              </div>
            </div>
            <ProductActionPanel product={product} />
          </div>
        </div>
      </div>
    </main>
  );
}
