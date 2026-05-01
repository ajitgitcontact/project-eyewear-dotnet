# Order Creation API Test Plan

Endpoint: `POST /api/orders/create`

Use `OrderCreationApiTests.http` for runnable HTTP scenarios. The cases below are the acceptance checklist for manual, smoke, or future automated integration tests.

## Authentication

| Case | Expected |
|------|----------|
| Unauthenticated request | `401` |
| Invalid JWT | `401` |
| User with wrong role | `403` |
| Valid `CUSTOMER` role | `201` when payload is valid |

## CustomerOrderId

| Case | Expected |
|------|----------|
| Frontend sends `customerOrderId` | Ignored |
| Backend-generated id | Matches `^[0-9]{11}$` and `YYMMDDXXXXX` |
| Same-day sequence increments | Later successful order has sequence + 1 |
| New server local day | First order starts at `00001` |
| Sequence above `99999` | `409` and no order rows committed |
| Duplicate id attempt | DB unique index prevents duplicate `Orders.CustomerOrderId` |
| Concurrent requests | No duplicate committed `CustomerOrderId` values |

## Validation

| Case | Expected |
|------|----------|
| Empty payload | `400` |
| Missing customer details | `400` |
| Missing address | `400` |
| Missing items | `400` |
| `quantity <= 0` | `400` |
| Invalid `productId` | `404` |
| Inactive product | `400` |
| Insufficient stock | `400` during validation or `409` during atomic inventory update |
| Invalid customization option | `400` |
| Invalid customization value | `400` |
| Customization value not belonging to option | `400` |
| Product supports prescription and prescription is missing | Valid; prescription is optional |
| Prescription provided when no selected product supports prescription | `400` |
| Prescription axis outside `0..180` | `400` |
| Negative `PD` | `400` |
| Invalid payment enum value | `400` model validation |

## Security

| Case | Expected |
|------|----------|
| Frontend `userId` | Ignored; response `order.userId` comes from JWT |
| Frontend `customerOrderId` | Ignored; response id is generated |
| Frontend `totalAmount` | Ignored |
| Frontend item `totalPrice` | Ignored |
| Frontend `discountAmount` | Ignored |
| Frontend `paymentStatus` | Ignored; order starts `PENDING` |
| Frontend `orderStatus` | Ignored; order starts `CREATED` |
| Frontend payment `status` | Ignored; payment starts `INITIATED` |
| Sensitive payment fields | Not accepted by DTO and should not appear in logs |

## Transaction

Verify through database queries after forced failures:

| Forced failure | Expected database state |
|----------------|-------------------------|
| Order item creation fails | No order row remains |
| Customization creation fails | No order or item rows remain |
| Address creation fails | No order, item, or customization rows remain |
| Payment creation fails | No order-related rows remain |
| Inventory update fails | No order-related rows remain |

## Discounts And Coupons

| Case | Expected |
|------|----------|
| No admin discount or coupon | `productDiscountTotal = 0`, `couponDiscountAmount = 0`, `finalAmount = originalSubtotal` |
| Active all-product admin discount | Applies automatically to eligible item unit prices |
| Active product-specific admin discount | Applies only to mapped products in `DiscountProducts` |
| Multiple applicable admin discounts | Best item-level discount is used per item |
| Valid coupon | Applies after product discounts |
| Invalid coupon | `400` |
| Expired coupon | `400` |
| Inactive coupon | `400` |
| Minimum order not satisfied | `400` |
| Global usage limit reached | `400` |
| Per-user usage limit reached | `400` |
| Percentage coupon with maximum amount | Coupon discount is capped |
| Discount greater than subtotal | Guard exists in `OrderCreationService.ValidateDiscountResult` |
| Frontend sends totals | Ignored; backend recalculates snapshots |

Snapshot fields to assert on successful responses:

- `originalSubtotal`
- `productDiscountTotal`
- `couponCode`
- `couponDiscountAmount`
- `discountAmount`
- `finalAmount`
- item `originalUnitPrice`
- item `productDiscountAmount`
- item `finalUnitPrice`
- item `finalLineTotal`

## Logging

Check `backend/logs/eyewear-YYYYMMDD.log`:

| Case | Expected |
|------|----------|
| Successful order | Logs request, user id, validation, id generation, totals, discount, each creation step, inventory update, commit |
| Failed order | Logs failure step and rollback |
| Payment data | No card number, CVV, UPI PIN, or secret data logged |

## Concurrency Smoke Test

1. Pick a product with stock greater than the request count.
2. Send multiple valid requests in parallel.
3. Confirm all successful responses have unique `customerOrderId`.
4. Confirm final product `AvailableQuantity` did not go negative.
5. Confirm failed concurrent stock races return `409` and leave no partial order rows.
