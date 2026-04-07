# Project Eyewear - .NET Backend

ASP.NET Core Web API with Entity Framework Core and Supabase PostgreSQL for an eyewear e-commerce platform.

## Prerequisites

| Dependency | Version | Install |
|---|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download/dotnet/9.0) | 9.0+ | Download from link |
| [EF Core CLI](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) | 9.0+ | `dotnet tool install --global dotnet-ef` |
| [Git](https://git-scm.com/) | 2.x+ | Download from link |
| [PostgreSQL database](https://supabase.com/) | 15+ | Supabase free tier or local PostgreSQL |

## NuGet Packages (auto-restored)

| Package | Version | Purpose |
|---|---|---|
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 9.0.x | PostgreSQL EF Core provider |
| `Microsoft.EntityFrameworkCore.Tools` | 9.0.x | EF Core migrations CLI |
| `Microsoft.AspNetCore.OpenApi` | 9.0.x | OpenAPI spec generation |
| `Swashbuckle.AspNetCore.SwaggerUI` | 10.1.7 | Swagger UI |
| `BCrypt.Net-Next` | 4.1.0 | Password hashing |
| `DotNetEnv` | 3.1.1 | `.env` file support |

## Project Structure

```
backend/
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
├── Services/
│   ├── UserService/                # IUserService + UserService
│   └── ProductsService/
│       ├── Interfaces/             # IProductService, IProductBusinessService,
│       │                           # ICustomizationOptionService, ICustomizationValueService,
│       │                           # IProductImageService, ICustomizationImageService
│       └── Services/               # Implementations for all interfaces
├── Migrations/                     # EF Core migrations
├── Tests/                          # ServiceTester (integration test runner)
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

This creates all tables: `Users`, `Products`, `CustomizationOptions`, `CustomizationValues`, `ProductImages`, `CustomizationImages`.

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
| `GET` | `/api/users` | Get all users |
| `GET` | `/api/users/{id}` | Get user by ID |
| `GET` | `/api/users/email/{email}` | Get user by email |
| `POST` | `/api/users` | Create a new user |
| `POST` | `/api/users/login` | Login (email + password) |
| `PUT` | `/api/users/{id}` | Update a user |
| `DELETE` | `/api/users/{id}` | Delete a user |

### Products API

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/products` | Create full product with customizations & images |
| `GET` | `/api/products` | Get all products (with nested data) |
| `GET` | `/api/products/{id}` | Get product by ID (with nested data) |
| `GET` | `/api/products/sku/{sku}` | Get product by SKU |
| `PUT` | `/api/products/{id}` | Update product details |
| `DELETE` | `/api/products/{id}` | Delete product (cascades all related data) |

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

## Tech Stack

- **.NET 9** — ASP.NET Core Web API (controller-based)
- **Entity Framework Core 9** — ORM with code-first migrations
- **Npgsql** — PostgreSQL provider for EF Core
- **Supabase** — Hosted PostgreSQL database
- **BCrypt.Net** — Password hashing
- **DotNetEnv** — Environment variable management
- **Swagger / OpenAPI** — API documentation
