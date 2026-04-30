# Order Fetch API Test Plan

## Complete Order API

Endpoint: `GET /api/orders/{customerOrderId}`

| Case | Expected |
|------|----------|
| No token | `401` |
| Wrong role | `403` |
| `CUSTOMER` fetches own order | `200` |
| `CUSTOMER` fetches another customer's order | `403` |
| `ADMIN` fetches any order | `200` |
| `SUPER_ADMIN` fetches any order | `200` |
| Customer response | Does not include transaction id or internal payment id |
| Admin response | Includes connected order details and current payment model fields |
| Invalid `customerOrderId` format | `400` |
| Non-existing `customerOrderId` | `404` |
| Order with no payment | `200`, empty payment list |
| Order with no prescription | `200`, empty prescription list |
| Multiple items | All items returned |
| Item customizations | Returned under each item |
| Status logs | Returned newest first from existing status log service |

## Admin Order List API

Endpoint: `GET /api/orders`

| Case | Expected |
|------|----------|
| No token | `401` |
| `CUSTOMER` role | `403` |
| `ADMIN` role | `200` |
| `SUPER_ADMIN` role | `200` |
| Filter by `fromCreatedDate` | Only matching orders |
| Filter by `toCreatedDate` | Only matching orders |
| Filter by `orderStatus` | Only matching orders |
| Filter by `paymentStatus` | Only matching orders |
| Filter by `customerOrderId` | Exact matching order |
| Filter by `email` | Partial email match |
| Filter by `contactNumber` | Partial phone match |
| Filter by `userId` | Only matching user orders |
| Pagination | Correct `totalCount`, `pageNumber`, `pageSize`, and page items |
| Invalid date range | `400` |
| Invalid page number | `400` |
| Invalid page size | `400` |

## Customer Order List API

Endpoint: `GET /api/customer/orders`

| Case | Expected |
|------|----------|
| No token | `401` |
| `ADMIN` or `SUPER_ADMIN` role | `403` |
| `CUSTOMER` role | `200` |
| Customer passes `userId` query | Ignored by model binding |
| Customer order scope | Only JWT user's orders |
| Filters | Date/status filters apply within JWT user's orders |
| Pagination | Correct `totalCount`, `pageNumber`, `pageSize`, and page items |
| Empty result | `200` with empty `orders` list |

## Security Checks

- Never accept customer user id from query or body.
- Always use JWT `NameIdentifier`.
- Validate complete-order ownership for every detail request.
- Do not expose confidential payment data in customer response.
- Do not log transaction id or payment secrets.
- Validate `CustomerOrderId` as `YYMMDDXXXXX`.
- Validate pagination inputs.
