import Link from "next/link";
import { Product } from "../lib/types";
import { formatUsd } from "../lib/currency";

interface ProductCardProps {
  product: Product;
}

export default function ProductCard({ product }: ProductCardProps) {
  const primaryImage = product.images.find((image) => image.isPrimary) ?? product.images[0];

  return (
    <article className="card">
      {primaryImage ? (
        <img className="cardImage" src={primaryImage.imageUrl} alt={product.name} />
      ) : (
        <div className="cardImage" style={{ display: "grid", placeItems: "center" }}>
          No image
        </div>
      )}
      <div className="cardBody">
        <div>
          <span className="badge">{product.category}</span>
          <h2 className="productName">{product.name}</h2>
          <p className="productMeta">SKU {product.sku} | {product.brand ?? "Brand not set"}</p>
        </div>
        <div>
          <p className="price">{formatUsd(product.basePrice)}</p>
          <p className="productMeta">{product.availableQuantity} in stock | {product.soldQuantity} sold</p>
        </div>
        <Link href={`/product/${product.productId}`} className="button">
          View product
        </Link>
      </div>
    </article>
  );
}
