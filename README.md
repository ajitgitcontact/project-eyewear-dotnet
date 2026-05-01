# Project Eyewear - .NET Backend

ASP.NET Core Web API with Entity Framework Core and Supabase PostgreSQL for an eyewear e-commerce platform.

## Prerequisites

| Dependency | Version | Install |
|---|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download/dotnet/10.0) | 10.0+ | Download from link |
| [EF Core CLI](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) | 9.0+ | `dotnet tool install --global dotnet-ef` |
| [Git](https://git-scm.com/) | 2.x+ | Download from link |
| [PostgreSQL database](https://supabase.com/) | 15+ | Supabase free tier or local PostgreSQL |

## NuGet Packages (auto-restored)

| Package | Version | Purpose |
|---|---|---|
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 9.0.x | PostgreSQL EF Core provider |
| `Microsoft.EntityFrameworkCore.Tools` | 9.0.x | EF Core migrations CLI |
| `Microsoft.AspNetCore.OpenApi` | 9.0.14 | OpenAPI spec generation |
| `Swashbuckle.AspNetCore.SwaggerUI` | 10.1.7 | Swagger UI |
| `BCrypt.Net-Next` | 4.1.0 | Password hashing |
| `DotNetEnv` | 3.1.1 | `.env` file support |
| `Serilog.AspNetCore` | 9.0.0 | ASP.NET Core structured logging |
| `Serilog.Sinks.Console` | 6.0.0 | Console log output |
| `Serilog.Sinks.File` | 7.0.0 | Rolling file log output |

## Project Structure

```
backend/
├── Application/
│   ├── Abstractions/
│   │   ├── Users/                  # IUserService
│   │   └── Products/               # Product and customization service interfaces
│   ├── DependencyInjection/        # Application layer DI registration
│   └── Exceptions/                 # App exceptions (NotFound/Conflict/etc.)
├── Controllers/                    # API controllers
│   ├── ProductsController.cs       # Products CRUD + customization endpoints
│   ├── UsersController.cs          # Users CRUD + login
│   └── DbTestController.cs        # DB connectivity test
├── Data/
│   └── AppDbContext.cs             # EF Core DbContext
├── DTOs/                           # Data Transfer Objects
│   ├── UserDtos/                   # CreateUser, UpdateUser, UserResponse, Login
│   ├── ProductDtos/                # CreateProduct, UpdateProduct, ProductResponse,
│   │                               # CreateFullProduct, FullProductResponse
│   ├── CustomizationOptionDtos/    # Create, Update, Response
│   ├── CustomizationValueDtos/     # Create, Update, Response
│   ├── ProductImageDtos/           # Create, Update, Response
│   └── CustomizationImageDtos/     # Create, Update, Response
├── Models/                         # Entity models
│   ├── User.cs
│   └── Products/                   # Product, CustomizationOption, CustomizationValue,
│                                   # ProductImage, CustomizationImage
├── Infrastructure/
│   ├── DependencyInjection/        # Infrastructure DI (DbContext, provider setup)
│   └── Services/
│       ├── Users/                  # UserService implementation
│       └── Products/               # Product and customization service implementations
├── Migrations/                     # EF Core migrations
├── wwwroot/images/products/        # Static product images
├── Program.cs                      # App entry point + DI configuration
├── .env                            # Environment variables (not in git)
├── .env.example                    # Template for .env
├── appsettings.json                # Base configuration
└── appsettings.Development.json    # Dev configuration
```

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/ajitgitcontact/project-eyewear-dotnet.git
cd project-eyewear-dotnet
```

### 2. Install EF Core CLI (if not installed)

```bash
dotnet tool install --global dotnet-ef
```

### 3. Restore NuGet packages

```bash
cd backend
dotnet restore
```

### 4. Configure environment variables

Create a `.env` file in the `backend/` folder (copy from the template):

```bash
cp .env.example .env
```

Edit `backend/.env` with your database connection string:

```
ConnectionStrings__DefaultConnection=Host=<POOLER_HOST>;Port=5432;Database=postgres;Username=<USERNAME>;Password=<PASSWORD>;SSL Mode=Require;Trust Server Certificate=true
```

Add JWT settings in `backend/.env` (required for authentication):

```
JWT__Key=<YOUR_LONG_RANDOM_SECRET>
JWT__Issuer=EyewearApi
JWT__Audience=EyewearApiUsers
JWT__ExpireMinutes=60
```

Use a strong key with at least 32 characters.

**Supabase connection tips:**
- Go to your Supabase project → **Settings → Database → Connection String**
- If you're on an **IPv4 network**, use the **Session Pooler** connection (not direct)
- The pooler host looks like: `aws-<region>.pooler.supabase.com`
- The pooler username includes your project ref: `postgres.<project-ref>`

**Local PostgreSQL alternative:**
```
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=eyewear;Username=postgres;Password=yourpassword
```

### 5. Apply database migrations

```bash
dotnet ef database update
```

This creates all tables including `Users`, product/customization tables, order tables, and `OrderNumberSequences`.

### 6. Run the application

```bash
dotnet run
```

The API starts at: **http://localhost:5047**

### 7. Verify it works

```bash
curl http://localhost:5047/api/dbtest
```

Or open [http://localhost:5047/swagger](http://localhost:5047/swagger) for Swagger UI.

## API Endpoints

### Swagger UI

Open [http://localhost:5047/swagger](http://localhost:5047/swagger) for interactive API docs.

### Users API

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/users` | Get all users (ADMIN, SUPER_ADMIN) |
| `GET` | `/api/users/{id}` | Get user by ID (ADMIN/SUPER_ADMIN or owner) |
| `GET` | `/api/users/email/{email}` | Get user by email (ADMIN, SUPER_ADMIN) |
| `POST` | `/api/users` | Create a new user (public CUSTOMER signup, admin can create any role) |
| `POST` | `/api/users/login` | Login (email + password) |
| `PUT` | `/api/users/{id}` | Update a user (ADMIN/SUPER_ADMIN or owner, no customer role escalation) |
| `DELETE` | `/api/users/{id}` | Delete a user (ADMIN, SUPER_ADMIN) |

### Auth API

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/auth/signup` | Customer signup endpoint (always creates CUSTOMER role) |

#### Login/Signup Response

Authentication endpoints return:

```json
{
  "token": "<jwt-token>",
  "role": "CUSTOMER"
}
```

JWT token contains these claims:
- `NameIdentifier` (User Id)
- `Email`
- `Role`

Token expiry is set to 1 hour.

#### Role Constants

- `SUPER_ADMIN`
- `ADMIN`
- `CUSTOMER`

#### Ownership Rule

For user profile endpoints, CUSTOMER can access only their own user record.

### Orders API

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/orders/create` | Create a complete checkout order in one transaction |

#### Authentication

`POST /api/orders/create` requires a Bearer JWT.

Allowed roles:

- `CUSTOMER`
- `ADMIN`
- `SUPER_ADMIN`

The frontend must not send `userId`. The backend reads the authenticated user id from the JWT `NameIdentifier` claim. Any frontend-provided `userId`, `customerOrderId`, `totalAmount`, item total, discount amount, `paymentStatus`, or `orderStatus` fields are not part of the request DTO and are ignored by model binding.

#### Request Shape

```json
{
  "customer": {
    "name": "Aarav Sharma",
    "email": "aarav@example.com",
    "phone": "9876543210"
  },
  "address": {
    "type": "SHIPPING",
    "line1": "221 MG Road",
    "line2": "Near Metro Station",
    "city": "Bengaluru",
    "state": "Karnataka",
    "pincode": "560001",
    "country": "India"
  },
  "items": [
    {
      "productId": 1,
      "quantity": 1,
      "customizations": [
        {
          "customizationOptionId": 1,
          "customizationValueId": 1
        }
      ]
    }
  ],
  "prescription": {
    "rightSphere": -1.25,
    "rightCylinder": -0.5,
    "rightAxis": 90,
    "leftSphere": -1.0,
    "leftCylinder": -0.25,
    "leftAxis": 80,
    "pd": 62,
    "notes": "Use latest prescription."
  },
  "payment": {
    "method": "COD",
    "transactionId": null
  },
  "couponCode": "WELCOME500",
  "notes": "Leave package with security."
}
```

Required fields:

- `customer.name`
- `customer.email`
- `address.line1`, `city`, `state`, `pincode`, `country`
- At least one item in `items`
- `items[].productId`
- `items[].quantity` greater than `0`
- `payment.method`

Optional fields:

- `customer.phone`
- `address.line2`
- `items[].customizations`
- `prescription`
- `payment.transactionId`
- `couponCode` or `discountCode`
- `notes`

#### Response Shape

```json
{
  "customerOrderId": "26050100001",
  "subtotal": 3000,
  "discountAmount": 0,
  "finalAmount": 3000,
  "order": {
    "ordersId": "orders_...",
    "customerOrderId": "26050100001",
    "userId": 12,
    "customerName": "Aarav Sharma",
    "customerEmail": "aarav@example.com",
    "customerPhone": "9876543210",
    "totalAmount": 3000,
    "paymentStatus": "PENDING",
    "orderStatus": "CREATED",
    "notes": "Leave package with security.",
    "createdAt": "2026-05-01T10:00:00Z",
    "updatedAt": "2026-05-01T10:00:00Z"
  },
  "address": {},
  "items": [
    {
      "item": {},
      "customizations": []
    }
  ],
  "prescription": {},
  "payment": {
    "paymentsId": "payments_...",
    "customerOrderId": "26050100001",
    "method": "COD",
    "transactionId": null,
    "amount": 3000,
    "status": "INITIATED",
    "createdAt": "2026-05-01T10:00:00Z",
    "updatedAt": "2026-05-01T10:00:00Z"
  },
  "statusLog": {}
}
```

#### CustomerOrderId Generation

`CustomerOrderId` is generated only by the backend in this format:

```
YYMMDDXXXXX
```

Example:

```
26050100001
```

The sequence resets daily and supports up to `99999` orders per server local date. The `OrderNumberSequences` table stores one row per date:

| Column | Purpose |
|---|---|
| `OrderNumberSequencesId` | Primary key |
| `SequenceDate` | Server local date for the sequence, unique |
| `LastSequenceNumber` | Last number issued for that date |
| `CreatedAt` | Row creation timestamp |
| `UpdatedAt` | Last sequence update timestamp |

Generation uses a PostgreSQL upsert on `SequenceDate`, so concurrent requests do not receive duplicate committed IDs. `Orders.CustomerOrderId` also has a unique index.

#### Pricing And Discounts

The backend calculates all pricing:

- Item unit price = `Products.BasePrice + selected CustomizationValues.AdditionalPrice`
- Original subtotal = sum of original item totals
- Active admin discounts from `Discounts` / `DiscountProducts` are applied automatically per item
- Product discount total = sum of item-level admin discounts
- Customer coupon discount is applied after product discounts when `couponCode` is provided
- Final amount = original subtotal - product discounts - coupon discount

The frontend must only send `couponCode`; it must not send or trust `originalSubtotal`, `productDiscountTotal`, `couponDiscountAmount`, item totals, or `finalAmount`. Those values are backend-calculated and stored as immutable order snapshots:

- `Orders.OriginalSubtotal`
- `Orders.ProductDiscountTotal`
- `Orders.CouponCode`
- `Orders.CouponDiscountAmount`
- `Orders.FinalAmount`
- `OrderItems.OriginalUnitPrice`
- `OrderItems.ProductDiscountAmount`
- `OrderItems.FinalUnitPrice`
- `OrderItems.FinalLineTotal`

Coupon validation checks active state, date window, minimum order amount, global usage limit, per-user usage limit, and maximum coupon amount for percentage coupons. Invalid/inactive/expired coupons fail with `400`.

Admin discount and coupon APIs:

| Method | Endpoint | Roles | Purpose |
|---|---|---|---|
| `GET` | `/api/admin/discounts` | `ADMIN`, `SUPER_ADMIN` | List admin discounts |
| `POST` | `/api/admin/discounts` | `ADMIN`, `SUPER_ADMIN` | Create all-product or product-specific discount |
| `PUT` | `/api/admin/discounts/{id}` | `ADMIN`, `SUPER_ADMIN` | Update discount |
| `DELETE` | `/api/admin/discounts/{id}` | `ADMIN`, `SUPER_ADMIN` | Delete discount |
| `GET` | `/api/admin/coupons` | `ADMIN`, `SUPER_ADMIN` | List coupons |
| `POST` | `/api/admin/coupons` | `ADMIN`, `SUPER_ADMIN` | Create coupon |
| `PUT` | `/api/admin/coupons/{id}` | `ADMIN`, `SUPER_ADMIN` | Update coupon |
| `DELETE` | `/api/admin/coupons/{id}` | `ADMIN`, `SUPER_ADMIN` | Delete coupon |

Order journey logs are stored in `OrderStatusLogs` with `EventType`, `LogMessage`, `PaymentStatus`, and `CreatedByUserId`. Admins can inspect them with `GET /api/orders/{customerOrderId}/logs`.

#### Validation Rules

The API validates:

- Authenticated user exists and is active.
- Payload contains customer, address, payment, and at least one item.
- Product exists, is active, and has enough available stock.
- Required product customizations are present.
- Customization option belongs to the selected product.
- Customization value belongs to the selected product and option.
- Prescription is optional when at least one selected product has `HasPrescription = true`.
- Prescription is rejected when none of the selected products support prescriptions.
- Prescription axis values must be `0` to `180`; `PD` must be non-negative.
- Final amount cannot be negative or inconsistent with subtotal and discount.

Payment and order statuses are backend-controlled:

- New orders start with `OrderStatus.CREATED`.
- New orders start with `PaymentStatus.PENDING`.
- New payment records start with `PaymentTxnStatus.INITIATED`.

#### Transaction And Rollback

The full order creation flow is wrapped in a database transaction. If any step fails, the transaction rolls back and no partial order data should remain.

Steps inside the transaction:

1. Validate request.
2. Generate `CustomerOrderId`.
3. Calculate totals.
4. Apply discount service.
5. Create order.
6. Create items.
7. Create item customizations.
8. Create address.
9. Create optional prescription.
10. Create payment.
11. Create status log.
12. Atomically update inventory.
13. Commit.

Inventory is updated with a conditional SQL update so concurrent orders cannot decrement stock below zero.

#### Logging

The controller logs request received, authenticated `UserId`, and completion. The business service logs validation, ID generation, totals, discount application, every creation step, transaction start/commit, success, and rollback failures.

Sensitive payment data is not logged. The service logs payment method/status and internal payment id, but not card data. The current request DTO does not accept card numbers, CVV, UPI PIN, or similar sensitive fields.

#### Error Responses

Errors are returned by `GlobalExceptionMiddleware` as:

```json
{
  "message": "Product not found.",
  "correlationId": "0HN..."
}
```

Common status codes:

| Status | Meaning |
|---|---|
| `400` | Invalid payload, missing required data, invalid customization, invalid prescription |
| `401` | Missing/invalid Bearer token |
| `403` | Authenticated user does not have an allowed role |
| `404` | User or product not found |
| `409` | Daily sequence exhausted, duplicate order id, or stock conflict |
| `500` | Unexpected server error |

Example failure:

```json
{
  "message": "Customization value does not belong to the provided customization option.",
  "correlationId": "0HN7ABC123"
}
```

### Order Fetch APIs

| Method | Endpoint | Roles | Description |
|---|---|---|---|
| `GET` | `/api/orders/{customerOrderId}` | CUSTOMER, ADMIN, SUPER_ADMIN | Fetch one complete order |
| `GET` | `/api/orders` | ADMIN, SUPER_ADMIN | Search/list all orders for admin panel |
| `GET` | `/api/customer/orders` | CUSTOMER | List authenticated customer's own orders |

#### GET `/api/orders/{customerOrderId}`

Fetches one complete order by backend-generated `CustomerOrderId`.

Rules:

- `customerOrderId` is required and trimmed.
- Format must be `YYMMDDXXXXX`, for example `26050100001`.
- `CUSTOMER` can fetch only their own order.
- `ADMIN` and `SUPER_ADMIN` can fetch any order.
- User id is always read from JWT; it is never accepted from query/body.

Customer response includes:

- Order summary
- Items and item customizations
- Addresses
- Prescriptions if available
- Status logs
- Limited payment details only: `method`, `status`, `amount`, `createdAt`

Admin/Super Admin response includes:

- Full order summary
- Items and item customizations
- Addresses
- Prescriptions
- Status logs
- Payment details from the current `Payments` model, including `paymentsId` and `transactionId`

Highly confidential payment secrets must not be added to response DTOs or logs. The current payment model does not contain card numbers, CVV, UPI PIN, gateway raw response, or similar secrets.

Example:

```http
GET /api/orders/26050100001
Authorization: Bearer <token>
```

Example customer payment response:

```json
{
  "customerPayments": [
    {
      "method": "COD",
      "status": "INITIATED",
      "amount": 3000,
      "createdAt": "2026-05-01T10:00:00Z"
    }
  ]
}
```

Example admin payment response:

```json
{
  "adminPayments": [
    {
      "paymentsId": "payments_...",
      "customerOrderId": "26050100001",
      "method": "COD",
      "transactionId": "provider-reference",
      "amount": 3000,
      "status": "INITIATED",
      "createdAt": "2026-05-01T10:00:00Z",
      "updatedAt": "2026-05-01T10:00:00Z"
    }
  ]
}
```

#### GET `/api/orders`

Admin/Super Admin order search for the admin order management page.

Query parameters:

| Parameter | Description |
|---|---|
| `fromCreatedDate` | Include orders where `CreatedAt >= fromCreatedDate` |
| `toCreatedDate` | Include orders where `CreatedAt <= toCreatedDate` |
| `orderStatus` | Filter by `CREATED`, `CONFIRMED`, `SHIPPED`, `DELIVERED`, `CANCELLED` |
| `paymentStatus` | Filter by `PENDING`, `PAID`, `FAILED` |
| `customerOrderId` | Exact customer order id |
| `email` | Partial customer email match |
| `contactNumber` | Partial customer phone match |
| `userId` | Exact user id |
| `pageNumber` | Default `1`; must be greater than `0` |
| `pageSize` | Default `20`; must be `1` to `100` |

Example:

```http
GET /api/orders?fromCreatedDate=2026-05-01&toCreatedDate=2026-05-31&orderStatus=CREATED&paymentStatus=PENDING&pageNumber=1&pageSize=20
Authorization: Bearer <admin-token>
```

Example response:

```json
{
  "totalCount": 42,
  "pageNumber": 1,
  "pageSize": 20,
  "orders": [
    {
      "ordersId": "orders_...",
      "customerOrderId": "26050100001",
      "userId": 12,
      "customerName": "Aarav Sharma",
      "customerEmail": "aarav@example.com",
      "customerPhone": "9876543210",
      "totalAmount": 3000,
      "paymentStatus": "PENDING",
      "orderStatus": "CREATED",
      "createdAt": "2026-05-01T10:00:00Z",
      "updatedAt": "2026-05-01T10:00:00Z"
    }
  ]
}
```

#### GET `/api/customer/orders`

Customer "My Orders" endpoint. It always uses the JWT user id and returns only orders where `Orders.UserId` matches the authenticated customer.

Query parameters:

| Parameter | Description |
|---|---|
| `fromCreatedDate` | Include orders where `CreatedAt >= fromCreatedDate` |
| `toCreatedDate` | Include orders where `CreatedAt <= toCreatedDate` |
| `orderStatus` | Optional order status filter |
| `paymentStatus` | Optional payment status filter |
| `pageNumber` | Default `1`; must be greater than `0` |
| `pageSize` | Default `20`; must be `1` to `100` |

Example:

```http
GET /api/customer/orders?pageNumber=1&pageSize=20
Authorization: Bearer <customer-token>
```

Example response:

```json
{
  "totalCount": 3,
  "pageNumber": 1,
  "pageSize": 20,
  "orders": [
    {
      "customerOrderId": "26050100001",
      "totalAmount": 3000,
      "paymentStatus": "PENDING",
      "orderStatus": "CREATED",
      "createdAt": "2026-05-01T10:00:00Z",
      "updatedAt": "2026-05-01T10:00:00Z"
    }
  ]
}
```

### Products API

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/products` | Create full product with customizations & images (ADMIN, SUPER_ADMIN) |
| `GET` | `/api/products` | Get all products (with nested data) - supports sorting |
| `GET` | `/api/products/{id}` | Get product by ID (with nested data) |
| `GET` | `/api/products/sku/{sku}` | Get product by SKU |
| `PUT` | `/api/products/{id}` | Update product details (ADMIN, SUPER_ADMIN) |
| `DELETE` | `/api/products/{id}` | Delete product (ADMIN, SUPER_ADMIN; cascades related data) |

#### Products Sorting

The **GET `/api/products`** endpoint supports sorting via the `sort` query parameter:

| Sort Option | Query Parameter | Behavior |
|---|---|---|
| **Price: Low to High** | `?sort=price_asc` | Ascending by base price (cheapest first) |
| **Price: High to Low** | `?sort=price_desc` | Descending by base price (expensive first) |
| **Popularity** | `?sort=popularity` | Descending by sold quantity (most sold first) |
| **What's New** | `?sort=newest` | Descending by created date (newest products first) |
| **Default** | `?sort=default` or no parameter | Ascending by priority column (featured products first) |

##### Example Requests:

```bash
# Default sort (by priority)
GET /api/products
GET /api/products?sort=default

# Price sorting
GET /api/products?sort=price_asc       # Lowest price first
GET /api/products?sort=price_desc      # Highest price first

# Popularity
GET /api/products?sort=popularity      # Most sold first

# What's new
GET /api/products?sort=newest          # Newest products first
```

### Customization Options API

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/products/{productId}/options` | Get all options for a product |
| `GET` | `/api/products/options/{optionId}` | Get option by ID |
| `POST` | `/api/products/{productId}/options` | Add customization option |
| `PUT` | `/api/products/options/{optionId}` | Update option |
| `DELETE` | `/api/products/options/{optionId}` | Delete option (cascades values) |

### Customization Values API

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/products/options/{optionId}/values` | Get all values for an option |
| `GET` | `/api/products/values/{valueId}` | Get value by ID |
| `POST` | `/api/products/options/{optionId}/values` | Add value to option |
| `PUT` | `/api/products/values/{valueId}` | Update value |
| `DELETE` | `/api/products/values/{valueId}` | Delete value |

### Product Images API

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/products/{productId}/images` | Get all images for a product |
| `GET` | `/api/products/images/{imageId}` | Get image by ID |
| `POST` | `/api/products/{productId}/images` | Add product image |
| `PUT` | `/api/products/images/{imageId}` | Update image |
| `DELETE` | `/api/products/images/{imageId}` | Delete image |

### Customization Images API

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/products/{productId}/customization-images` | Get all customization images for a product |
| `GET` | `/api/products/customization-images/{imageId}` | Get customization image by ID |
| `POST` | `/api/products/{productId}/customization-images` | Add customization image |
| `PUT` | `/api/products/customization-images/{imageId}` | Update customization image |
| `DELETE` | `/api/products/customization-images/{imageId}` | Delete customization image |

### DB Test

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/dbtest` | Test database connection |

## Example: Create a Full Product

```bash
curl -X POST http://localhost:5047/api/products \
  -H "Content-Type: application/json" \
  -d '{
    "sku": "EW-AVI-001",
    "name": "Classic Aviator",
    "description": "Premium aviator sunglasses",
    "brand": "EyeWear Pro",
    "category": "Sunglasses",
    "basePrice": 199.99,
    "hasPrescription": false,
    "images": [
      { "imageUrl": "/images/products/aviator-front.jpg", "isPrimary": true, "displayOrder": 1 }
    ],
    "customizationOptions": [
      {
        "name": "Frame Color",
        "isRequired": true,
        "displayOrder": 1,
        "values": [
          {
            "value": "Gold",
            "additionalPrice": 0,
            "customizationImages": [
              { "imageUrl": "/images/products/aviator-gold.jpg" }
            ]
          },
          {
            "value": "Silver",
            "additionalPrice": 10,
            "customizationImages": [
              { "imageUrl": "/images/products/aviator-silver.jpg" }
            ]
          }
        ]
      },
      {
        "name": "Lens Type",
        "isRequired": true,
        "displayOrder": 2,
        "values": [
          { "value": "Polarized", "additionalPrice": 50, "customizationImages": [] },
          { "value": "Photochromic", "additionalPrice": 75, "customizationImages": [] }
        ]
      }
    ]
  }'
```

## Logging & Monitoring

### Structured Logging with Serilog

All API requests and service operations are logged with structured data for easy querying and debugging:

- **Console Sink:** Real-time log output in development
- **File Sink:** Rolling daily log files in `backend/logs/eyewear-YYYYMMDD.log` with 14-day retention
- **Correlation IDs:** Track request flow across services using `X-Correlation-ID` header

#### Log Format

```
[HH:mm:ss Level] (CorrelationId) SourceContext - Message

Example:
[14:23:45 INF] (abc123def456) backend.Services.UserService.UserService - Output: Found=true, Email=user@example.com
```

#### Middleware

1. **CorrelationIdMiddleware** — Generates or reads `X-Correlation-ID` header to track requests
   - All logs within a request share the same correlation ID
   - Correlation ID echoed in response headers

2. **GlobalExceptionMiddleware** — Catches all unhandled exceptions and returns structured JSON error responses
   - Maps exceptions to appropriate HTTP status codes (400, 409, 500)
   - Includes correlation ID in error response for log correlation

#### Log Levels

- **Production:** `Information` (important events only)
- **Development:** `Debug` (verbose output for application code, `Information` for frameworks)

#### Viewing Logs

```bash
# Watch logs in real-time (development)
dotnet run  # Check console output

# Read historical logs
cat backend/logs/eyewear-2026-04-09.log

# Filter by correlation ID
grep "abc123def456" backend/logs/eyewear-*.log
```

#### Configuration

Logging is configured in `appsettings.json` and `appsettings.Development.json`:
- Console output template includes timestamp, level, correlation ID, source, and message
- File output includes full timestamp with timezone
- Daily rolling files with 14-day retention (older files auto-deleted)

## Tech Stack

- **.NET 10 / `net10.0`** — ASP.NET Core Web API (controller-based)
- **Entity Framework Core 9 packages** — ORM with code-first migrations
- **Npgsql** — PostgreSQL provider for EF Core
- **Supabase** — Hosted PostgreSQL database
- **Serilog 9.0.0** — Structured logging with Console and File sinks
- **BCrypt.Net** — Password hashing
- **DotNetEnv** — Environment variable management
- **Swagger / OpenAPI** — API documentation
