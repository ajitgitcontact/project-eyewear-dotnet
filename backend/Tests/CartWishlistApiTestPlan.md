# Cart And Wishlist API Test Plan

Use `backend.http` for runnable HTTP examples. Replace IDs and tokens with values from a migrated database.

## Cart Authentication And Ownership

| Case | Expected |
|------|----------|
| No token on `/api/cart` | `401` |
| Invalid JWT | `401` |
| `ADMIN` or `SUPER_ADMIN` token | `403` |
| Valid `CUSTOMER` token | `200` |
| Frontend sends `userId` | Ignored; cart ownership comes from JWT |

## Cart Item Flow

| Case | Expected |
|------|----------|
| Empty active cart | `200` with empty cart response |
| Add active product with stock | Cart item created |
| Add same product/configuration again | Quantity increases when practical |
| Add inactive product | `400` |
| Add missing product | `404` |
| Quantity less than 1 | `400` model validation |
| Insufficient stock | `400` |
| Required customization missing | `400` |
| Invalid customization option/value | `400` |
| Prescription on non-prescription product | `400` |
| Prescription omitted for `HasPrescription=true` product | Valid; prescription is optional |
| Update quantity | Revalidates stock and recalculates preview totals |
| Remove item | Soft-removes item with `IsActive=false` |
| Clear cart | Removes active items and coupon preview, keeps cart row |

## Cart Coupon Preview

| Case | Expected |
|------|----------|
| Apply valid coupon to non-empty cart | Stores `CartCoupons` preview row and returns updated totals |
| Apply invalid coupon | `400` |
| Apply coupon to empty cart | `400` |
| Remove coupon | Coupon preview removed and totals recalculated |
| Coupon becomes invalid later | Preview is removed during recalculation |

## Cart Checkout

| Case | Expected |
|------|----------|
| Checkout empty cart | `400` |
| Checkout active cart | Creates order through `OrderCreationService` |
| Checkout with new `Idempotency-Key` | Creates one order and stores key on the order |
| Retry checkout with same `Idempotency-Key` after success | Returns the same order even if cart is already `CHECKED_OUT` |
| Double-click checkout with same `Idempotency-Key` | Does not create duplicate orders |
| Key longer than 100 chars | `400` |
| Checkout recalculation | Backend recalculates product prices, discounts, coupon, inventory, payment, and order logs |
| Successful checkout | Cart status becomes `CHECKED_OUT`, `CustomerOrderId` is stored, `CheckedOutAt` is set |
| Add item after checkout | New active cart is created |
| Reuse checked-out cart | Blocked by status rule |
| Multiple different prescription snapshots | `400`, because current order creation supports one order-level prescription |
| Checkout failure | Order transaction rolls back; cart is not marked checked out |

## Wishlist

| Case | Expected |
|------|----------|
| No token on `/api/wishlist` | `401` |
| `ADMIN` or `SUPER_ADMIN` token | `403` |
| Empty wishlist | `200` with empty list |
| Add active product | Wishlist item created |
| Add duplicate product | No duplicate row due unique `WishlistId + ProductId` |
| Add inactive product | `400` |
| Add missing product | `404` |
| Remove own wishlist item | Item removed |
| Remove another user's wishlist item | `404` from scoped lookup |
| Move directly addable product to cart | Adds cart item and removes wishlist item |
| Move product requiring customization/prescription | Returns `requiresProductConfiguration=true`; wishlist item remains |

## Logging

Check `backend/logs/eyewear-YYYYMMDD.log`:

- Cart creation started/completed
- Add/update/remove/clear cart events
- Coupon applied/removed
- Cart checkout started/completed/failed
- Wishlist created, item added, duplicate attempt, removed, moved to cart
- No JWT token, secrets, or confidential payment data logged
