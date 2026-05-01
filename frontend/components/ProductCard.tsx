import Link from "next/link";
import { Product } from "../lib/types";
import { formatUsd } from "../lib/currency";
import { demoProductImageSrc } from "../lib/demoProductImage";

interface ProductCardProps {
  product: Product;
}

export default function ProductCard({ product }: ProductCardProps) {
  return (
    <article className="card">
      <img className="cardImage" src={demoProductImageSrc} alt={product.name} />
      <div className="cardBody">
        <div>
          <span className="badge">{product.category}</span>
          <h2 className="productName">{product.name}</h2>
          <p className="productMeta">{product.brand ?? "Brand not set"}</p>
        </div>
        <div>
          <p className="price">{formatUsd(product.basePrice)}</p>
          {product.availableQuantity <= 0 ? <p className="productMeta">Out of stock</p> : null}
        </div>
        <Link href={`/product/${product.productId}`} className="button">
          View product
        </Link>
      </div>
    </article>
  );
}
