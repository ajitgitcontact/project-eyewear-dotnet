# Project Eyewear .NET Backend - Technical Documentation
**Documentation Date: April 9, 2026**

---

## 1. Project Overview

### Technology Stack
- **.NET 10 / `net10.0`** ASP.NET Core Web API (Controller-based)
- **Entity Framework Core 9 packages** with Npgsql PostgreSQL driver
- **PostgreSQL** Database via Supabase Session Pooler
- **Serilog 9.0.0** Structured Logging with Console and File sinks
- **BCrypt.Net-Next 4.1.0** Password hashing
- **DotNetEnv** Environment variable loading from .env file

### Development Environment
- **IDE:** Visual Studio Code
- **Runtime:** .NET 10.0 SDK/runtime (`10.0.203` verified locally)
- **Dev Server:** http://localhost:5047
- **Swagger UI:** http://localhost:5047/swagger
- **Database:** PostgreSQL via Supabase (pool connection: aws-1-ap-northeast-1.pooler.supabase.com:5432)

### Key Infrastructure Features
- **Correlation ID Middleware:** Generates/reads X-Correlation-ID header for request tracing
- **Global Exception Middleware:** Centralized error handling with structured JSON responses
- **Structured Logging:** All services instrumented with log entry/exit points and parameter logging
- **Dependency Injection:** All services registered as Scoped (per HTTP request)

---

## 2. Database Schema

### Table: Users
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INT | PK, AUTO_INCREMENT | Primary key, auto-generated |
| FirstName | VARCHAR(100) | NOT NULL | User's first name |
| LastName | VARCHAR(100) | NOT NULL | User's last name |
| Email | VARCHAR(150) | NOT NULL, UNIQUE, EmailAddress | User's email (unique constraint) |
| ContactNumber | VARCHAR(20) | NULLABLE, Phone validation | Optional phone number |
| PasswordHash | TEXT | NOT NULL | BCrypt hashed password (never plain text) |
| UserRole | VARCHAR(50) | NOT NULL | Role: "Customer", "Admin", etc. Default: "Customer" |
| IsActive | BOOLEAN | NOT NULL | Account active status. Default: true |
| CreatedAt | DATETIME | NOT NULL | Timestamp of user creation. Default: UTC Now |
| UpdatedAt | DATETIME | NULLABLE | Timestamp of last update. Auto-set on save |
| LastLoginAt | DATETIME | NULLABLE | Timestamp of most recent login. Set on LoginAsync |

**Indexes:**
- UNIQUE INDEX on Email (enforced by EF HasIndex configuration)

**Sample Data:**
- 5 existing users (verified via GET /api/users endpoint)

---

### Table: Products
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| ProductId | INT | PK, AUTO_INCREMENT | Primary key, auto-generated |
| SKU | VARCHAR(50) | NOT NULL, UNIQUE | Stock Keeping Unit (unique constraint) |
| Name | VARCHAR(200) | NOT NULL | Product name/title |
| Description | TEXT | NULLABLE | Detailed product description |
| Brand | VARCHAR(100) | NULLABLE | Brand name (e.g., "RAY-BAN", "OAKLEY") |
| Category | VARCHAR(100) | NOT NULL | Product category (e.g., "Sunglasses", "Reading") |
| BasePrice | DECIMAL(10,2) | NOT NULL | Base product price |
| HasPrescription | BOOLEAN | DEFAULT FALSE | Whether prescription lens is available |
| IsActive | BOOLEAN | NOT NULL | Product availability. Default: true |
| CreatedAt | DATETIME | NOT NULL | Timestamp of product creation. Default: UTC Now |
| UpdatedAt | DATETIME | NULLABLE | Timestamp of last update. Auto-set on save |

**Indexes:**
- UNIQUE INDEX on SKU (enforced by EF HasIndex configuration)

**Foreign Keys:**
- One-to-Many: Product → ProductImages (Cascade Delete)
- One-to-Many: Product → CustomizationOptions (Cascade Delete)
- One-to-Many: Product → CustomizationImages (Cascade Delete)

---

### Table: ProductImages
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| ProductImageId | INT | PK, AUTO_INCREMENT | Primary key, auto-generated |
| ProductId | INT | FK, NOT NULL | Foreign key to Products table |
| ImageUrl | VARCHAR(500) | NOT NULL | URL or path to image |
| IsPrimary | BOOLEAN | DEFAULT FALSE | Whether this is the main product image |
| DisplayOrder | INT | DEFAULT 0 | Sort order for image carousel |
| CreatedAt | DATETIME | NOT NULL | Timestamp of record creation |

**Foreign Keys:**
- Many-to-One: ProductImages → Products (FK: ProductId, OnDelete: Cascade)

---

### Table: CustomizationOptions
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| CustomizationOptionId | INT | PK, AUTO_INCREMENT | Primary key, auto-generated |
| ProductId | INT | FK, NOT NULL | Foreign key to Products table |
| Name | VARCHAR(100) | NOT NULL | Option name (e.g., "Color", "Size", "Material") |
| IsRequired | BOOLEAN | DEFAULT FALSE | Whether customer must select an option value |
| DisplayOrder | INT | DEFAULT 0 | Sort order for options in UI |
| CreatedAt | DATETIME | NOT NULL | Timestamp of record creation |
| UpdatedAt | DATETIME | NULLABLE | Timestamp of last update |

**Foreign Keys:**
- Many-to-One: CustomizationOptions → Products (FK: ProductId, OnDelete: Cascade)
- One-to-Many: CustomizationOption → CustomizationValues (Cascade Delete)

---

### Table: CustomizationValues
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| CustomizationValueId | INT | PK, AUTO_INCREMENT | Primary key, auto-generated |
| CustomizationOptionId | INT | FK, NOT NULL | Foreign key to CustomizationOptions table |
| Value | VARCHAR(150) | NOT NULL | Specific value (e.g., "Black", "M", "Plastic") |
| AdditionalPrice | DECIMAL(10,2) | DEFAULT 0.00 | Price adjustment for this value (can be positive or negative) |
| CreatedAt | DATETIME | NOT NULL | Timestamp of record creation |
| UpdatedAt | DATETIME | NULLABLE | Timestamp of last update |

**Foreign Keys:**
- Many-to-One: CustomizationValues → CustomizationOptions (FK: CustomizationOptionId, OnDelete: Cascade)
- One-to-Many: CustomizationValue → CustomizationImages (with Restrict Delete) |

---

### Table: CustomizationImages
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| CustomizationImageId | INT | PK, AUTO_INCREMENT | Primary key, auto-generated |
| ProductId | INT | FK, NOT NULL | Foreign key to Products table |
| CustomizationOptionId | INT | FK NOT NULL | Foreign key to CustomizationOptions table |
| CustomizationValueId | INT | FK, NOT NULL | Foreign key to CustomizationValues table |
| ImageUrl | VARCHAR(500) | NOT NULL | URL/path to preview image showing product with selected option+value |
| CreatedAt | DATETIME | NOT NULL | Timestamp of record creation |

**Foreign Keys:**
- Many-to-One: CustomizationImages → Products (FK: ProductId, OnDelete: Cascade)
- Many-to-One: CustomizationImages → CustomizationOptions (FK: CustomizationOptionId, OnDelete: **Restrict**)
- Many-to-One: CustomizationImages → CustomizationValues (FK: CustomizationValueId, OnDelete: **Restrict**)

**Notes:**
- Product deletion cascades to CustomizationImages automatically
- Option/Value deletion is **restricted** (prevent orphaned preview images showing invalid combinations)
- This 3-way join allows the system to store preview images for every valid option+value combination

---

## 3. Service Layer Architecture

### Service Hierarchy

```
UsersController
  ↓ (depends on)
IUserService ← UserService


ProductsController
  ├─ IProductBusinessService ← ProductBusinessService
  ├─ IProductService ← ProductService
  ├─ ICustomizationOptionService ← CustomizationOptionService
  ├─ ICustomizationValueService ← CustomizationValueService
  ├─ IProductImageService ← ProductImageService
  └─ ICustomizationImageService ← CustomizationImageService
```

---

### 3.1 UserService

**File:** `backend/Services/UserService/UserService.cs`

**Dependency Injection:**
```csharp
private readonly AppDbContext _context;
private readonly ILogger<UserService> _logger;
```

**Methods:**

| Method | Signature | Logic | Logging |
|--------|-----------|-------|---------|
| **GetAllAsync** | `Task<List<UserResponseDto>>` | Fetch all users from DB, map to DTO | Input: none; Output: Count={count} |
| **GetByIdAsync** | `Task<UserResponseDto?> (int userId)` | Query user by PK, null if not found | Input: UserId={id}; Output: Found={bool}, Email (masked) |
| **GetByEmailAsync** | `Task<UserResponseDto?> (string email)` | Query user by email, null if not found | Input: Email={email}; Output: Found={bool}, UserId |
| **CreateUserAsync** | `Task<UserResponseDto> (CreateUserDto dto)` | Validate email uniqueness → BCrypt hash password → insert → map to DTO | Input: Name, Role; Output: UserId, Email |
| **UpdateUserAsync** | `Task<UserResponseDto> (int userId, UpdateUserDto dto)` | Find user → update fields → set UpdatedAt → SaveChanges | Input: UserId, FirstName, LastName; Output: UserId, UpdatedAt |
| **LoginAsync** | `Task<LoginResponseDto> (LoginDto dto)` | Find user by email → BCrypt.Verify password → set LastLoginAt → return user data | Input: Email; Output: Authenticated={bool}, UserId, Role, LastLoginAt |
| **DeleteUserAsync** | `Task (int userId)` | Find user → delete → SaveChanges | Input: UserId; Output: Deleted={bool} |

**Security Notes:**
- Passwords never logged (checked via BCrypt.Verify, never in logs)
- Email appears in Input logging but not exposed in error responses
- Roles and UserIds visible in Authenticated logs (non-sensitive)

**Error Handling:**
- InvalidOperationException on duplicate email (409 Conflict via middleware)
- ArgumentException on invalid password format (400 Bad Request)

---

### 3.2 ProductService

**File:** `backend/Services/ProductsService/Services/ProductService.cs`

**Dependency Injection:**
```csharp
private readonly AppDbContext _context;
private readonly ILogger<ProductService> _logger;
```

**Methods:**

| Method | Signature | Logic | Logging |
|--------|-----------|-------|---------|
| **GetAllAsync** | `Task<List<ProductResponseDto>>` | Fetch all products from DB, project to DTO with related images | Input: none; Output: Count={count} |
| **GetByIdAsync** | `Task<ProductResponseDto?> (int productId)` | Query by PK with .Include(ProductImages), null if not found | Input: ProductId={id}; Output: Found={bool}, SKU, Name, Price |
| **GetBySkuAsync** | `Task<ProductResponseDto?> (string sku)` | Query by unique SKU, null if not found | Input: SKU={sku}; Output: Found={bool}, ProductId, Name |
| **CreateAsync** | `Task<ProductResponseDto> (CreateProductDto dto)` | Validate SKU uniqueness → insert Product → SaveChanges → map to DTO | Input: SKU, Name, Brand, Category, BasePrice; Output: ProductId, SKU |
| **UpdateAsync** | `Task<ProductResponseDto> (int productId, UpdateProductDto dto)` | Find product → update scalar fields → set UpdatedAt → SaveChanges | Input: ProductId, SKU, Name, Brand; Output: ProductId, UpdatedAt |
| **DeleteAsync** | `Task (int productId)` | Find product → remove (cascades to ProductImages, CustomizationOptions) → SaveChanges | Input: ProductId; Output: Deleted={bool} |
| **IncludePrimaryImage** | Helper method | Filters ProductImages to fetch only IsProimary=true for each product | Used internally in GetAll/GetById |

**Logging Details:**
- Input logs capture: SKU (string), Name (string), Brand (string), Category (string), BasePrice (decimal)
- Output logs capture: ProductId (int), SKU (string), Found (bool), image count (int)

**Error Handling:**
- InvalidOperationException on duplicate SKU (409 Conflict)
- EntityNotFoundException on delete of non-existent product (404 Not Found)

---

### 3.3 ProductBusinessService

**File:** `backend/Services/ProductsService/Services/ProductBusinessService.cs`

**Dependency Injection:**
```csharp
private readonly AppDbContext _context;
private readonly ILogger<ProductBusinessService> _logger;
```

**Methods:**

| Method | Signature | Logic | Logging |
|--------|-----------|-------|---------|
| **CreateFullProductAsync** | `Task<FullProductResponseDto> (CreateFullProductDto dto)` | **Transaction**: (1) Insert Product (2) Insert ProductImages (3) For each CustomizationOption: insert option → for each CustomizationValue: insert value → for each CustomizationImage: insert image (4) CommitAsync or RollbackAsync on exception | Input: SKU, Name, Brand, ImageCount, OptionCount; Output: Images={count}, Options={count}, CreatedAt |

**Nested Create Logic (nested logging):**
```
CreateFullProductAsync
  ├─ BeginTransaction
  ├─ Insert Product → logs "Product created. ProductId={id}"
  ├─ Insert ProductImages (loop) → logs "Image {i} created. PrimaryImageUrl"
  ├─ For each CustomizationOption
  │   ├─ Insert CustomizationOption → logs "Option created. OptionId={id}, Name='{name}'"
  │   └─ For each CustomizationValue
  │       └─ Insert CustomizationValue → logs "Value created. ValueId={id}, Value='{value}', Price={price}"
  ├─ For each CustomizationImage
  │   └─ Insert CustomizationImage → logs "Image created. ProductId={pid}, OptionId={oid}, ValueId={vid}"
  └─ CommitAsync → logs "Transaction committed. Images={count}, Options={count}"
```

**Error Handling:**
- Any exception during transaction triggers RollbackAsync (logged)
- InvalidOperationException on model validation failures (400 Bad Request)
- Returns structured FullProductResponseDto with nested Options array (each with Values array)

---

### 3.4 CustomizationOptionService

**File:** `backend/Services/ProductsService/Services/CustomizationOptionService.cs`

**Dependency Injection:**
```csharp
private readonly AppDbContext _context;
private readonly ILogger<CustomizationOptionService> _logger;
```

**Methods:**

| Method | Signature | Logic | Logging |
|--------|-----------|-------|---------|
| **GetByProductIdAsync** | `Task<List<CustomizationOptionResponseDto>> (int productId)` | Query all options for product with .Include(Values), return DTO list | Input: ProductId={id}; Output: Count={count} |
| **GetByIdAsync** | `Task<CustomizationOptionResponseDto?> (int optionId)` | Query by PK with .Include(Values), null if not found | Input: OptionId={id}; Output: Found={bool}, Name, IsRequired |
| **CreateAsync** | `Task<CustomizationOptionResponseDto> (CreateCustomizationOptionDto dto)` | Validate ProductId exists (FK check) → insert → SaveChanges → map DTO | Input: ProductId, Name, IsRequired; Output: OptionId, DisplayOrder |
| **UpdateAsync** | `Task<CustomizationOptionResponseDto> (int optionId, UpdateCustomizationOptionDto dto)` | Find option → update Name, IsRequired, DisplayOrder → SaveChanges | Input: OptionId, Name; Output: UpdatedAt |
| **DeleteAsync** | `Task (int optionId)` | Find option (validates exists) → soft delete or hard delete → SaveChanges | Input: OptionId; Output: Deleted={bool} |

**Validation Logging:**
- If ProductId FK validation fails: logs "FK validation failed. Product not found. ProductId={id}"
- All CRUD operations log with ProductId, OptionId, Name, IsRequired parameters

---

### 3.5 CustomizationValueService

**File:** `backend/Services/ProductsService/Services/CustomizationValueService.cs`

**Dependency Injection:**
```csharp
private readonly AppDbContext _context;
private readonly ILogger<CustomizationValueService> _logger;
```

**Methods:**

| Method | Signature | Logic | Logging |
|--------|-----------|-------|---------|
| **GetByOptionIdAsync** | `Task<List<CustomizationValueResponseDto>> (int optionId)` | Query all values for customization option, return DTO list | Input: OptionId={id}; Output: Count={count} |
| **GetByIdAsync** | `Task<CustomizationValueResponseDto?> (int valueId)` | Query by PK, null if not found | Input: ValueId={id}; Output: Found={bool}, Value, AdditionalPrice |
| **CreateAsync** | `Task<CustomizationValueResponseDto> (CreateCustomizationValueDto dto)` | Validate OptionId exists (FK check) → insert → SaveChanges → map DTO | Input: OptionId, Value, AdditionalPrice; Output: ValueId |
| **UpdateAsync** | `Task<CustomizationValueResponseDto> (int valueId, UpdateCustomizationValueDto dto)` | Find value → update Value, AdditionalPrice → SaveChanges | Input: ValueId, Value; Output: UpdatedAt |
| **DeleteAsync** | `Task (int valueId)` | Find value → delete (restricted by FK if CustomizationImages reference it) → SaveChanges | Input: ValueId; Output: Deleted={bool} |

**Validation Logging:**
- If OptionId FK validation fails: logs "FK validation failed. CustomizationOption not found. OptionId={id}"
- All logs include ValueId, Value (string), AdditionalPrice (decimal) for traceability

---

### 3.6 ProductImageService

**File:** `backend/Services/ProductsService/Services/ProductImageService.cs`

**Dependency Injection:**
```csharp
private readonly AppDbContext _context;
private readonly ILogger<ProductImageService> _logger;
```

**Methods:**

| Method | Signature | Logic | Logging |
|--------|-----------|-------|---------|
| **GetByProductIdAsync** | `Task<List<ProductImageResponseDto>> (int productId)` | Query all images for product ordered by DisplayOrder | Input: ProductId={id}; Output: Count={count} |
| **GetByIdAsync** | `Task<ProductImageResponseDto?> (int imageId)` | Query by PK, null if not found | Input: ImageId={id}; Output: Found={bool}, IsPrimary, DisplayOrder |
| **CreateAsync** | `Task<ProductImageResponseDto> (CreateProductImageDto dto)` | Validate ProductId exists (FK check) → if IsPrimary=true, set other images to IsPrimary=false → insert → SaveChanges | Input: ProductId, ImageUrl, IsPrimary; Output: ImageId, DisplayOrder |
| **UpdateAsync** | `Task<ProductImageResponseDto> (int imageId, UpdateProductImageDto dto)` | Find image → update URL, DisplayOrder, IsPrimary → SaveChanges | Input: ImageId, IsPrimary; Output: UpdatedAt |
| **DeleteAsync** | `Task (int imageId)` | Find image → delete → SaveChanges | Input: ImageId; Output: Deleted={bool} |

**Logging Details:**
- Input logs: ProductId, ImageUrl (first 100 chars), IsPrimary flag
- Output logs: ImageId, DisplayOrder (position in carousel), Created timestamp

---

### 3.7 CustomizationImageService

**File:** `backend/Services/ProductsService/Services/CustomizationImageService.cs`

**Dependency Injection:**
```csharp
private readonly AppDbContext _context;
private readonly ILogger<CustomizationImageService> _logger;
```

**Methods:**

| Method | Signature | Logic | Logging |
|--------|-----------|-------|---------|
| **GetByProductIdAsync** | `Task<List<CustomizationImageResponseDto>> (int productId)` | Query all customization images for product | Input: ProductId={id}; Output: Count={count} |
| **GetByOptionValueAsync** | `Task<List<CustomizationImageResponseDto>> (int optionId, int valueId)` | Query customization images matching both OptionId and ValueId | Input: OptionId, ValueId; Output: Count={count} |
| **GetByIdAsync** | `Task<CustomizationImageResponseDto?> (int imageId)` | Query by PK, null if not found | Input: ImageId={id}; Output: Found={bool}, OptionId, ValueId |
| **CreateAsync** | `Task<CustomizationImageResponseDto> (CreateCustomizationImageDto dto)` | **Validate all 3 FKs exist**: (1) ProductId in Products table (2) CustomizationOptionId in CustomizationOptions table (3) CustomizationValueId in CustomizationValues table → insert → SaveChanges → map DTO | Input: ProductId, OptionId, ValueId, ImageUrl; Output: ImageId |
| **UpdateAsync** | `Task<CustomizationImageResponseDto> (int imageId, UpdateCustomizationImageDto dto)` | Find image → update ImageUrl → SaveChanges | Input: ImageId, ImageUrl; Output: UpdatedAt |
| **DeleteAsync** | `Task (int imageId)` | Find image → delete → SaveChanges | Input: ImageId; Output: Deleted={bool} |

**Most Comprehensive Validation (CreateAsync):**
```
CreateAsync({ProductId, OptionId, ValueId, ImageUrl})
  ├─ Log "Input: ProductId={pid}, OptionId={oid}, ValueId={vid}"
  ├─ Query: Product exists? 
  │   └─ if (!found) → Log "FK validation failed. Product not found. ProductId={pid}" → throw
  ├─ Query: CustomizationOption exists AND belongs to ProductId?
  │   └─ if (!found) → Log "FK validation failed. CustomizationOption not found. OptionId={oid}" → throw
  ├─ Query: CustomizationValue exists AND belongs to OptionId?
  │   └─ if (!found) → Log "FK validation failed. CustomizationValue not found. ValueId={vid}" → throw
  ├─ Insert CustomizationImage record
  └─ Log "Output: ImageId={iid}, ProductId={pid}, OptionId={oid}, ValueId={vid}"
```

---

### 3.8 DataSeeder (Added Apr 10, 2026)

**File:** `backend/Infrastructure/Services/DataSeeder.cs`

**Purpose:** Automatically seed 20 dummy products in development environment for testing sorting and API functionality.

**Dependency Injection:**
```csharp
private readonly AppDbContext _context;
private readonly ILogger<DataSeeder> _logger;
```

**Methods:**

| Method | Signature | Logic | Logging |
|--------|-----------|-------|---------|
| **SeedProductsAsync** | `Task` | Checks product count; clears existing products if < 20; generates and inserts 20 dummy products | Input: none; Output: Count={seeded_count} |
| **GenerateDummyProducts** | `List<Product>` | Creates 20 Product records with: RandomVarying prices ($102-$935), varying sold quantities (0-197), priorities (0-4), spread creation dates (last 20 days) | Output: List<Product> with 20 items |

**Seeding Logic:**
```
SeedProductsAsync
  ├─ Count existing products
  ├─ if (count >= 20)
  │   └─ Log "Products already exist. Skipping seeding." → return
  ├─ if (count > 0 && count < 20)
  │   └─ Log "Clearing {count} existing products for fresh seeding."
  │       └─ Delete all products (cascades CustomizationOptions, ProductImages, CustomizationImages)
  ├─ GenerateDummyProducts() → List<Product>
  │   └─ For each of 20 products:
  │       ├─ SKU = "PRD-{i:D6}" (e.g., "PRD-000001")
  │       ├─ Name = cycling through predefined names (Classic Black, Summer Blue, etc.)
  │       ├─ Brand = random from list (Ray-Ban, Aviator, Oakley, Prada, Gucci, Versace, Calvin Klein, Fossil)
  │       ├─ Category = random from list (Sunglasses, Reading Glasses, Fashion, Sports, Vintage)
  │       ├─ BasePrice = random decimal $99-$999 + random cents
  │       ├─ AvailableQuantity = random 5-100
  │       ├─ SoldQuantity = random 0-200 (for popularity testing)
  │       ├─ Priority = (i-1) % 5 (cycles 0-4 for priority distribution)
  │       ├─ CreatedAt = now - (20-i) days (spreads over last 20 days for newest sort testing)
  │       └─ HasPrescription = random boolean
  ├─ AddRangeAsync(products)
  ├─ SaveChangesAsync()
  └─ Log "Successfully seeded {count} products."
```

**Integration with Program.cs:**
```csharp
// In Program.cs, during app startup in Development environment:
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
        await seeder.SeedProductsAsync();
    }
}
```

**DI Registration (in InfrastructureServiceCollectionExtensions.cs):**
```csharp
services.AddScoped<DataSeeder>();
```

---

### 3.9 ProductSortOption (Added Apr 10, 2026)

**File:** `backend/Application/Abstractions/Products/ProductSortOption.cs`

**Purpose:** Centralized constants and validation logic for product sorting options.

**Static Members:**

| Member | Type | Value | Description |
|--------|------|-------|-------------|
| PriceAscending | const string | "price_asc" | Sort by BasePrice ascending |
| PriceDescending | const string | "price_desc" | Sort by BasePrice descending |
| Popularity | const string | "popularity" | Sort by SoldQuantity descending |
| Newest | const string | "newest" | Sort by CreatedAt descending |
| Default | const string | "default" | Sort by Priority ascending |
| ValidOptions | HashSet<string> | {price_asc, price_desc, popularity, newest, default} | All valid sort options |

**Static Methods:**

| Method | Signature | Purpose | Returns |
|--------|-----------|---------|---------|
| **IsValid** | `bool (string? sortOption)` | Validates if sort option is in ValidOptions set | true/false |
| **Normalize** | `string (string? sortOption)` | Converts to lowercase, returns Default if null/empty | normalized string |

**Usage in Controller:**
```csharp
if (!ProductSortOption.IsValid(sort))
{
    return BadRequest(new { message = "Invalid sort option..." });
}
var normalizedSort = ProductSortOption.Normalize(sort);
var products = await _businessService.GetAllFullProductsAsync(normalizedSort);
```

**Usage in Service:**
```csharp
private IQueryable<Product> ApplySorting(IQueryable<Product> query, string sortOption)
{
    return sortOption switch
    {
        ProductSortOption.PriceAscending => query.OrderBy(p => p.BasePrice),
        ProductSortOption.PriceDescending => query.OrderByDescending(p => p.BasePrice),
        ProductSortOption.Popularity => query.OrderByDescending(p => p.SoldQuantity),
        ProductSortOption.Newest => query.OrderByDescending(p => p.CreatedAt),
        ProductSortOption.Default or _ => query.OrderBy(p => p.Priority).ThenByDescending(p => p.CreatedAt),
    };
}
```

---

## 4. Controllers

### 4.1 UsersController

**File:** `backend/Controllers/UsersController.cs`

**Dependency Injection:**
```csharp
private readonly IUserService _userService;
private readonly ILogger<UsersController> _logger;
```

**Endpoints:**

| HTTP Method | Route | Method | Request Body | Response Body | Status Codes |
|---|---|---|---|---|---|
| GET | `/api/users` | GetAllUsersAsync | _(none)_ | `List<UserResponseDto>` | 200 OK, 500 |
| GET | `/api/users/{id}` | GetUserByIdAsync | _(none)_ | `UserResponseDto` | 200 OK, 404 Not Found, 500 |
| POST | `/api/users` | CreateUserAsync | `CreateUserDto` | `UserResponseDto` | 201 Created, 400 Bad Request, 409 Conflict (duplicate email), 500 |
| POST | `/api/users/login` | LoginAsync | `LoginDto` | `LoginResponseDto` | 200 OK, 401 Unauthorized, 400 Bad Request, 500 |
| PUT | `/api/users/{id}` | UpdateUserAsync | `UpdateUserDto` | `UserResponseDto` | 200 OK, 404 Not Found, 409 Conflict, 500 |
| DELETE | `/api/users/{id}` | DeleteUserAsync | _(none)_ | `{ "message": "User deleted" }` | 204 No Content, 404 Not Found, 500 |

**Logging Pattern (all endpoints):**
```csharp
_logger.LogInformation("Request received. Input: {{InputParams}}");
// ... call service ...
_logger.LogInformation("Succeeded. Output: {{OutputParams}}");
```

**Error Responses (via GlobalExceptionMiddleware):**
```json
{
  "message": "Error description",
  "correlationId": "abc123def456"
}
```

---

**GET /api/users/GetAllUsersAsync — Fetch All Users**
- **Request:** No request body
- **Response:** `List<UserResponseDto>` with 5+ users
- **Logic:** Service.GetAllAsync() → map each User to UserResponseDto
- **Logging:** Input: none; Output: Count={count}

---

**GET /api/users/{id}/GetUserByIdAsync — Fetch Single User**
- **Request:** Route parameter `id` (int, required)
- **Response:** Single `UserResponseDto` or 404
- **Logic:** Service.GetByIdAsync(id) → check null → return DTO or NotFound
- **Logging:** Input: UserId={id}; Output: Found={bool}, Email (partially masked)

---

**POST /api/users/CreateUserAsync — Create New User**
- **Request Body:**
  ```json
  {
    "firstName": "John",
    "lastName": "Doe",
    "email": "john@example.com",
    "contactNumber": "+1-234-567-8900",
    "userRole": "Customer"
  }
  ```
- **Response:** `UserResponseDto` with generated Id (201 Created)
- **Validation:**
  - Email must be unique (409 Conflict if duplicate)
  - Email format must be valid
  - FirstName, LastName required
- **Logic:** Service.CreateUserAsync(dto) → BCrypt hash password → insert → return DTO
- **Logging:** Input: Name, Role; Output: UserId, Email

---

**POST /api/users/login/LoginAsync — User Login**
- **Request Body:**
  ```json
  {
    "email": "john@example.com",
    "password": "SecurePassword123!"
  }
  ```
- **Response:** `LoginResponseDto` with user data and LastLoginAt timestamp (200 OK)
- **Validation:**
  - Email must exist
  - Password must match BCrypt hash
  - User must be IsActive=true
- **Logic:** Service.LoginAsync(dto) → verify password → set LastLoginAt → return DTO
- **Logging:** Input: Email; Output: Authenticated={bool}, UserId, Role, LastLoginAt
- **Security:** Password never logged; only authentication result

---

**PUT /api/users/{id}/UpdateUserAsync — Update User**
- **Request:**
  - Route parameter `id` (int, required)
  - **Request Body:**
    ```json
    {
      "firstName": "Jane",
      "lastName": "Smith",
      "contactNumber": "+1-234-567-8901"
    }
    ```
- **Response:** Updated `UserResponseDto` (200 OK)
- **Validation:**
  - User with id must exist (404 if not)
  - Email uniqueness preserved
- **Logic:** Service.UpdateUserAsync(id, dto) → update fields → set UpdatedAt → SaveChanges
- **Logging:** Input: UserId, FirstName, LastName; Output: UserId, UpdatedAt

---

**DELETE /api/users/{id}/DeleteUserAsync — Delete User**
- **Request:** Route parameter `id` (int, required)
- **Response:** 204 No Content (no body)
- **Validation:**
  - User with id must exist (404 if not)
- **Logic:** Service.DeleteUserAsync(id) → remove from DB → SaveChanges
- **Logging:** Input: UserId; Output: Deleted={bool}

---

### 4.2 ProductsController

**File:** `backend/Controllers/ProductsController.cs`

**Dependency Injection:**
```csharp
private readonly IProductBusinessService _productBusinessService;
private readonly IProductService _productService;
private readonly ICustomizationOptionService _optionService;
private readonly ICustomizationValueService _valueService;
private readonly IProductImageService _imageService;
private readonly ICustomizationImageService _customizationImageService;
private readonly ILogger<ProductsController> _logger;
```

**Endpoints Overview (25+ total):**

#### Product CRUD (5 endpoints)

| HTTP Method | Route | Method | Request/Response | Status Codes |
|---|---|---|---|---|
| GET | `/api/products` | GetAllProductsAsync | Query: `?sort={option}` Response: `List<FullProductResponseDto>` | 200, 400, 500 |
| GET | `/api/products/{id}` | GetProductByIdAsync | Response: `FullProductResponseDto` | 200, 404, 500 |
| GET | `/api/products/sku/{sku}` | GetProductBySkuAsync | Response: `ProductResponseDto` | 200, 404, 500 |
| POST | `/api/products` | CreateFullProductAsync | Request: `CreateFullProductDto` | 201, 400, 409, 500 |
| PUT | `/api/products/{id}` | UpdateProductAsync | Request: `UpdateProductDto` | 200, 404, 500 |
| DELETE | `/api/products/{id}` | DeleteProductAsync | Response: 204 No Content | 204, 404, 500 |

**GET /api/products Sorting Feature (Added Apr 10, 2026):**
- **Query Parameter:** `sort` (string, optional, defaults to "default")
- **Valid Sort Options:**
  - `price_asc` — Ascending by BasePrice (low to high)
  - `price_desc` — Descending by BasePrice (high to low)
  - `popularity` — Descending by SoldQuantity (most sold first)
  - `newest` — Descending by CreatedAt (newest first)
  - `default` — Ascending by Priority, then descending by CreatedAt
- **Error Handling:**
  - 400 Bad Request if invalid sort option provided
  - Response includes helpful error message listing valid options
- **Logging:**
  - Input: SortOption={option}
  - Output: Count={count}, SortOption={normalized_option}
- **Implementation:**
  - Query parameter validated via `ProductSortOption.IsValid()` in controller
  - Sort applied at database level in `ApplySorting()` method in service
  - Returns `IEnumerable<FullProductResponseDto>` with sorted results
- **Service Methods:**
  - Overloaded: `GetAllFullProductsAsync()` (no sort params, defaults to default)
  - Overloaded: `GetAllFullProductsAsync(string sortOption)` (with sort parameter)

---

#### Customization Option Endpoints (6 endpoints)

| HTTP | Route | Method | Request/Response | Status |
|---|---|---|---|---|
| GET | `/api/products/{productId}/options` | GetOptionsByProductIdAsync | Response: `List<CustomizationOptionResponseDto>` | 200, 404, 500 |
| GET | `/api/products/options/{optionId}` | GetOptionByIdAsync | Response: `CustomizationOptionResponseDto` | 200, 404, 500 |
| POST | `/api/products/{productId}/options` | CreateOptionAsync | Request: `CreateCustomizationOptionDto` | 201, 400, 404, 500 |
| PUT | `/api/products/options/{optionId}` | UpdateOptionAsync | Request: `UpdateCustomizationOptionDto` | 200, 404, 500 |
| DELETE | `/api/products/options/{optionId}` | DeleteOptionAsync | 204 No Content | 204, 404, 500 |

---

#### Customization Value Endpoints (6 endpoints)

| HTTP | Route | Method | Request/Response | Status |
|---|---|---|---|---|
| GET | `/api/products/options/{optionId}/values` | GetValuesByOptionIdAsync | Response: `List<CustomizationValueResponseDto>` | 200, 404, 500 |
| GET | `/api/products/values/{valueId}` | GetValueByIdAsync | Response: `CustomizationValueResponseDto` | 200, 404, 500 |
| POST | `/api/products/options/{optionId}/values` | CreateValueAsync | Request: `CreateCustomizationValueDto` | 201, 400, 404, 500 |
| PUT | `/api/products/values/{valueId}` | UpdateValueAsync | Request: `UpdateCustomizationValueDto` | 200, 404, 500 |
| DELETE | `/api/products/values/{valueId}` | DeleteValueAsync | 204 No Content | 204, 404, 500 |

---

#### Product Image Endpoints (6 endpoints)

| HTTP | Route | Method | Request/Response | Status |
|---|---|---|---|---|
| GET | `/api/products/{productId}/images` | GetImagesByProductIdAsync | Response: `List<ProductImageResponseDto>` | 200, 404, 500 |
| GET | `/api/products/images/{imageId}` | GetImageByIdAsync | Response: `ProductImageResponseDto` | 200, 404, 500 |
| POST | `/api/products/{productId}/images` | CreateImageAsync | Request: `CreateProductImageDto` | 201, 400, 404, 500 |
| PUT | `/api/products/images/{imageId}` | UpdateImageAsync | Request: `UpdateProductImageDto` | 200, 404, 500 |
| DELETE | `/api/products/images/{imageId}` | DeleteImageAsync | 204 No Content | 204, 404, 500 |

---

#### Customization Image Endpoints (6 endpoints)

| HTTP | Route | Method | Request/Response | Status |
|---|---|---|---|---|
| GET | `/api/products/{productId}/customization-images` | GetCustomizationImagesByProductIdAsync | Response: `List<CustomizationImageResponseDto>` | 200, 404, 500 |
| GET | `/api/products/options/{optionId}/values/{valueId}/images` | GetCustomizationImagesByOptionValueAsync | Response: `List<CustomizationImageResponseDto>` | 200, 404, 500 |
| GET | `/api/products/customization-images/{imageId}` | GetCustomizationImageByIdAsync | Response: `CustomizationImageResponseDto` | 200, 404, 500 |
| POST | `/api/products/customization-images` | CreateCustomizationImageAsync | Request: `CreateCustomizationImageDto` | 201, 400, 404, 500 |
| PUT | `/api/products/customization-images/{imageId}` | UpdateCustomizationImageAsync | Request: `UpdateCustomizationImageDto` | 200, 404, 500 |
| DELETE | `/api/products/customization-images/{imageId}` | DeleteCustomizationImageAsync | 204 No Content | 204, 404, 500 |

---

**Key Endpoint: CreateFullProductAsync (POST /api/products)**
- **Purpose:** Transactional creation of complete product hierarchy in single request
- **Request Body:**
  ```json
  {
    "product": {
      "sku": "RAY-BAN-001",
      "name": "Ray-Ban Aviator",
      "description": "Classic aviator sunglasses",
      "brand": "RAY-BAN",
      "category": "Sunglasses",
      "basePrice": 150.00,
      "hasPrescription": true,
      "isActive": true
    },
    "images": [
      {
        "imageUrl": "https://example.com/image1.jpg",
        "isPrimary": true,
        "displayOrder": 0
      }
    ],
    "customizationOptions": [
      {
        "name": "Color",
        "isRequired": true,
        "displayOrder": 0,
        "values": [
          {
            "value": "Black",
            "additionalPrice": 0.00
          },
          {
            "value": "Gold",
            "additionalPrice": 50.00
          }
        ]
      }
    ],
    "customizationImages": [
      {
        "customizationOptionId": 1,
        "customizationValueId": 1,
        "imageUrl": "https://example.com/product-black.jpg"
      }
    ]
  }
  ```
- **Response:** `FullProductResponseDto` (201 Created)
- **Atomicity:** All-or-nothing transaction (rollback on any error)

---

## 5. Data Transfer Objects (DTOs)

### 5.1 User DTOs

**CreateUserDto** (Request body for POST /api/users)
```csharp
public class CreateUserDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? ContactNumber { get; set; }
    public string UserRole { get; set; } = "Customer";
    public string Password { get; set; } = string.Empty;
}
```

**UpdateUserDto** (Request body for PUT /api/users/{id})
```csharp
public class UpdateUserDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ContactNumber { get; set; }
}
```

**UserResponseDto** (Response body for all user endpoints)
```csharp
public class UserResponseDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? ContactNumber { get; set; }
    public string UserRole { get; set; } = "Customer";
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}
```

**LoginDto** (Request body for POST /api/users/login)
```csharp
public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
```

**LoginResponseDto** (Response body for POST /api/users/login)
```csharp
public class LoginResponseDto
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserRole { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime LastLoginAt { get; set; }
}
```

---

### 5.2 Product DTOs

**CreateProductDto** (Request body for POST /api/products when creating product alone)
```csharp
public class CreateProductDto
{
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Brand { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public bool HasPrescription { get; set; } = false;
}
```

**UpdateProductDto** (Request body for PUT /api/products/{id})
```csharp
public class UpdateProductDto
{
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Brand { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public bool HasPrescription { get; set; }
}
```

**ProductResponseDto** (Response body for single product fetch)
```csharp
public class ProductResponseDto
{
    public int ProductId { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Brand { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public bool HasPrescription { get; set; }
    public bool IsActive { get; set; }
    public List<ProductImageResponseDto> Images { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

**CreateFullProductDto** (Request body for POST /api/products with nested entities)
```csharp
public class CreateFullProductDto
{
    public CreateProductDto Product { get; set; } = new();
    public List<CreateProductImageDto> Images { get; set; } = new();
    public List<CreateCustomizationOptionDto> CustomizationOptions { get; set; } = new();
    public List<CreateCustomizationImageDto> CustomizationImages { get; set; } = new();
}
```

**FullProductResponseDto** (Response body for CreateFullProductAsync)
```csharp
public class FullProductResponseDto
{
    public ProductResponseDto Product { get; set; } = new();
    public List<CustomizationOptionResponseDto> CustomizationOptions { get; set; } = new();
    public List<ProductImageResponseDto> Images { get; set; } = new();
}
```

---

### 5.3 Customization Option DTOs

**CreateCustomizationOptionDto** (Request body for POST /api/products/{productId}/options)
```csharp
public class CreateCustomizationOptionDto
{
    public string Name { get; set; } = string.Empty;
    public bool IsRequired { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;
    public List<CreateCustomizationValueDto> Values { get; set; } = new();
}
```

**UpdateCustomizationOptionDto** (Request body for PUT /api/products/options/{optionId})
```csharp
public class UpdateCustomizationOptionDto
{
    public string Name { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }
}
```

**CustomizationOptionResponseDto** (Response body for option endpoints)
```csharp
public class CustomizationOptionResponseDto
{
    public int CustomizationOptionId { get; set; }
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }
    public List<CustomizationValueResponseDto> Values { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

---

### 5.4 Customization Value DTOs

**CreateCustomizationValueDto** (Request body for POST /api/products/options/{optionId}/values)
```csharp
public class CreateCustomizationValueDto
{
    public string Value { get; set; } = string.Empty;
    public decimal AdditionalPrice { get; set; } = 0.00m;
}
```

**UpdateCustomizationValueDto** (Request body for PUT /api/products/values/{valueId})
```csharp
public class UpdateCustomizationValueDto
{
    public string Value { get; set; } = string.Empty;
    public decimal AdditionalPrice { get; set; }
}
```

**CustomizationValueResponseDto** (Response body for value endpoints)
```csharp
public class CustomizationValueResponseDto
{
    public int CustomizationValueId { get; set; }
    public int CustomizationOptionId { get; set; }
    public string Value { get; set; } = string.Empty;
    public decimal AdditionalPrice { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

---

### 5.5 Product Image DTOs

**CreateProductImageDto** (Request body for POST /api/products/{productId}/images)
```csharp
public class CreateProductImageDto
{
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;
}
```

**UpdateProductImageDto** (Request body for PUT /api/products/images/{imageId})
```csharp
public class UpdateProductImageDto
{
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
}
```

**ProductImageResponseDto** (Response body for image endpoints)
```csharp
public class ProductImageResponseDto
{
    public int ProductImageId { get; set; }
    public int ProductId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

---

### 5.6 Customization Image DTOs

**CreateCustomizationImageDto** (Request body for POST /api/products/customization-images)
```csharp
public class CreateCustomizationImageDto
{
    public int ProductId { get; set; }
    public int CustomizationOptionId { get; set; }
    public int CustomizationValueId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}
```

**UpdateCustomizationImageDto** (Request body for PUT /api/products/customization-images/{imageId})
```csharp
public class UpdateCustomizationImageDto
{
    public string ImageUrl { get; set; } = string.Empty;
}
```

**CustomizationImageResponseDto** (Response body for customization image endpoints)
```csharp
public class CustomizationImageResponseDto
{
    public int CustomizationImageId { get; set; }
    public int ProductId { get; set; }
    public int CustomizationOptionId { get; set; }
    public int CustomizationValueId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
```

---

## 6. Logging Architecture

### 6.1 Serilog Configuration

**appsettings.json** (Base configuration, inherited by all environments)
```json
{
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] ({CorrelationId}) {SourceContext} - {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/eyewear-.log",
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] ({CorrelationId}) {SourceContext} - {Message:lj}{NewLine}{Exception}",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 14
        }
      }
    ],
    "Enrich": [
      "FromLogContext"
    ]
  }
}
```

**appsettings.Development.json** (Dev-specific overrides)
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft.EntityFrameworkCore": "Information",
        "backend": "Debug"
      }
    }
  }
}
```

### 6.2 Middleware Implementation

**CorrelationIdMiddleware.cs** — Request scoped correlation tracking
```csharp
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        string correlationId = context.Request.Headers
            .TryGetValue("X-Correlation-ID", out var header) 
            ? header.ToString() 
            : Guid.NewGuid().ToString("N");

        context.TraceIdentifier = correlationId;
        context.Response.Headers["X-Correlation-ID"] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}
```

**GlobalExceptionMiddleware.cs** — Centralized error handling
```csharp
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (InvalidOperationException ex)
        {
            // 409 Conflict (e.g., duplicate email, duplicate SKU)
            _logger.LogWarning(ex, "Conflict: {Message}", ex.Message);
            context.Response.StatusCode = 409;
            await WriteErrorResponse(context, "Conflict", ex.Message);
        }
        catch (Exception ex)
        {
            // 500 Internal Server Error (unexpected exceptions)
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            context.Response.StatusCode = 500;
            await WriteErrorResponse(context, "Internal Server Error", ex.Message);
        }
    }

    private static async Task WriteErrorResponse(HttpContext context, string type, string message)
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            message = message,
            correlationId = context.TraceIdentifier
        };
        await context.Response.WriteAsJsonAsync(response);
    }
}
```

### 6.3 Program.cs Middleware Registration

```csharp
// ... configuration code ...

app.UseMiddleware<CorrelationIdMiddleware>();  // Must run first
app.UseMiddleware<GlobalExceptionMiddleware>();  // Must run after Correlation
app.UseSerilogRequestLogging();  // Log all HTTP requests

app.UseHttpsRedirection();
app.UseStaticFiles();
app.MapControllers();

// ... remaining middleware ...
```

### 6.4 Service Logging Pattern

**Input/Output Pattern:**
```csharp
public async Task<UserResponseDto> GetByIdAsync(int userId)
{
    _logger.LogInformation("Input: UserId={UserId}", userId);
    
    var user = await _context.Users.FindAsync(userId);
    
    _logger.LogInformation("Output: Found={Found}, Email={Email}", 
        user != null, user?.Email ?? "N/A");
    
    return user == null ? null : MapToDto(user);
}
```

**Parameter Examples by Service:**
- **UserService:** Email (masked), Role, UserId, Authenticated, IsActive, LastLoginAt
- **ProductService:** SKU, Name, Brand, Category, BasePrice, ProductId, IsActive
- **ProductBusinessService:** Transaction lifecycle, ImageCount, OptionCount, NestedEntityIds
- **CustomizationOptionService:** ProductId, OptionId, Name, IsRequired, DisplayOrder
- **CustomizationValueService:** OptionId, ValueId, Value, AdditionalPrice
- **ProductImageService:** ProductId, ImageId, IsPrimary, DisplayOrder
- **CustomizationImageService:** ProductId, OptionId, ValueId, FKValidationStatus

### 6.5 Log File Examples

**Log file:** `backend/logs/eyewear-20260409.log` (rolling daily retention, 14-day limit)

**Sample entries:**
```
[2026-04-09 14:23:45.123 +00:00 INF] (abc1def2ghi3) backend.Controllers.UsersController - Request received. Input: 
[2026-04-09 14:23:45.456 +00:00 INF] (abc1def2ghi3) backend.Services.UserService.UserService - Output: Count=5
[2026-04-09 14:23:45.789 +00:00 INF] (abc1def2ghi3) backend.Controllers.UsersController - Succeeded.

[2026-04-09 14:24:12.321 +00:00 INF] (def4ghi5jkl6) backend.Services.ProductsService.Services.ProductBusinessService - Input: SKU=RAY-001, Name='Ray-Ban Aviator', Brand='RAY-BAN', ImageCount=2, OptionCount=1
[2026-04-09 14:24:12.654 +00:00 INF] (def4ghi5jkl6) backend.Services.ProductsService.Services.ProductBusinessService - Option created. OptionId=15, Name='Color'
[2026-04-09 14:24:12.987 +00:00 INF] (def4ghi5jkl6) backend.Services.ProductsService.Services.ProductBusinessService - Output: Images=2, Options=1
```

---

## 7. Code Flow Diagrams

### 7.1 User Registration & Login Flow

```
[Client POST /api/users]
         ↓
[UsersController.CreateUserAsync]
  ├─ Log Input: Name, Role
  ├─ Call IUserService.CreateUserAsync(dto)
  │   ↓
  │   [UserService.CreateUserAsync]
  │   ├─ Validate Email uniqueness via EF query
  │   ├─ BCrypt.HashPassword(plaintext_password) → PasswordHash
  │   ├─ Insert User record → SaveChanges
  │   ├─ Log Output: UserId, Email
  │   └─ Return UserResponseDto
  │
  └─ Log Output: Succeeded
      Return 201 Created + DTO

[Client POST /api/users/login]
         ↓
[UsersController.LoginAsync]
  ├─ Log Input: Email
  ├─ Call IUserService.LoginAsync(dto)
  │   ↓
  │   [UserService.LoginAsync]
  │   ├─ Query User by Email
  │   ├─ BCrypt.Verify(plaintext, hash) → true/false
  │   ├─ if true: Update LastLoginAt → SaveChanges
  │   ├─ Log Output: Authenticated={result}, UserId, Role, LastLoginAt
  │   └─ Return LoginResponseDto or null
  │
  └─ Log Output: Succeeded or 401 Unauthorized
      Return 200 OK + DTO or 401 Unauthorized
```

### 7.2 Full Product Creation Flow (Transaction)

```
[Client POST /api/products with nested data]
         ↓
[ProductsController.CreateFullProductAsync]
  ├─ Log Input: 
  ├─ Call IProductBusinessService.CreateFullProductAsync(dto)
  │   ↓
  │   [ProductBusinessService.CreateFullProductAsync]
  │   ├─ BeginTransaction
  │   │
  │   ├─ Insert Product (from dto.Product)
  │   │   └─ Log: Product created. ProductId={id}
  │   │
  │   ├─ For each ProductImage in dto.Images:
  │   │   ├─ Insert ProductImage
  │   │   └─ Log: Image {i} created. BaseUrl={url}
  │   │
  │   ├─ For each CustomizationOption in dto.CustomizationOptions:
  │   │   ├─ Insert CustomizationOption
  │   │   ├─ Log: Option created. OptionId={id}, Name={name}
  │   │   │
  │   │   └─ For each CustomizationValue in option.Values:
  │   │       ├─ Insert CustomizationValue
  │   │       └─ Log: Value created. ValueId={id}, Value={val}, Price={p}
  │   │
  │   ├─ For each CustomizationImage in dto.CustomizationImages:
  │   │   ├─ Validate: Product exists
  │   │   ├─ Validate: CustomizationOption exists
  │   │   ├─ Validate: CustomizationValue exists
  │   │   ├─ Insert CustomizationImage
  │   │   └─ Log: Image created. ProdId={pid}, OptId={oid}, ValId={vid}
  │   │
  │   ├─ CommitAsync
  │   ├─ Log: Transaction committed. Images={count}, Options={count}
  │   └─ Return FullProductResponseDto
  │
  └─ If any exception:
      ├─ RollbackAsync (all changes discarded)
      ├─ Log Error: Transaction rolled back
      └─ Return 400/409/500 error
```

### 7.3 Customization Image Creation Flow (3-Way FK Validation)

```
[Client POST /api/products/customization-images]
     ↓
[ProductsController.CreateCustomizationImageAsync]
  ├─ Call ICustomizationImageService.CreateAsync(dto)
  │   ↓
  │   [CustomizationImageService.CreateAsync]
  │   ├─ Log Input: ProductId={pid}, OptionId={oid}, ValueId={vid}
  │   │
  │   ├─ FK Validation 1: Query Product by ProductId
  │   │   └─ if (!found)
  │   │       ├─ Log Error: FK validation failed. Product not found. ProductId={pid}
  │   │       └─ throw InvalidOperationException (409 Conflict)
  │   │
  │   ├─ FK Validation 2: Query CustomizationOption by OptionId
  │   │   └─ if (!found)
  │   │       ├─ Log Error: FK validation failed. CustomizationOption not found. OptionId={oid}
  │   │       └─ throw InvalidOperationException (409 Conflict)
  │   │
  │   ├─ FK Validation 3: Query CustomizationValue by ValueId
  │   │   └─ if (!found)
  │   │       ├─ Log Error: FK validation failed. CustomizationValue not found. ValueId={vid}
  │   │       └─ throw InvalidOperationException (409 Conflict)
  │   │
  │   ├─ All FK checks passed:
  │   │   ├─ Insert CustomizationImage record
  │   │   ├─ SaveChanges
  │   │   ├─ Log Output: ImageId={iid}, ProductId={pid}, OptionId={oid}, ValueId={vid}
  │   │   └─ Return CustomizationImageResponseDto
  │   │
  │   └─ On exception: log error, throw
  │
  └─ GlobalExceptionMiddleware catches exception
      ├─ Log correlationId + error message
      └─ Return JSON error response with correlationId
```

---

## 8. Error Handling Strategy

### HTTP Status Codes

| Status | Scenario | Example |
|--------|----------|---------|
| 200 OK | Successful GET/PUT/POST (when not 201) | LoginAsync, UpdateUser |
| 201 Created | Successful POST that creates resource | CreateUser, CreateProduct |
| 204 No Content | Successful DELETE | DeleteUser, DeleteProduct |
| 400 Bad Request | Invalid input validation (missing required fields, invalid format) | Email format invalid in CreateUser |
| 401 Unauthorized | Authentication failed (wrong password) | Login with incorrect password |
| 404 Not Found | Resource doesn't exist | GET /api/users/999 (user doesn't exist) |
| 409 Conflict | Duplicate unique constraint or FK validation fails | CreateUser with existing email, CreateCustomizationImage with invalid ProductId |
| 500 Internal Server Error | Unhandled exceptions | Unexpected DB error, null reference |

### Exception Mapping (GlobalExceptionMiddleware)

| Exception Type | HTTP Status | Response Body |
|---|---|---|
| InvalidOperationException | 409 Conflict | `{ "message": "explanation", "correlationId": "..." }` |
| DbUpdateException | 409 Conflict (if FK/unique) or 400 Bad Request | Same as above |
| Exception (all others) | 500 Internal Server Error | Same as above |

### Validation Approach

**Database-Level Constraints:**
- Email UNIQUE constraint on Users table
- SKU UNIQUE constraint on Products table
- CustomizationImages FK constraints with RESTRICT/CASCADE

**Application-Level Validation (before DB write):**
- Email format validation (DataAnnotations [EmailAddress])
- Required field checks
- FK existence checks in CustomizationImageService (3-way validation)

---

## 9. Deployment & Configuration

### Environment Setup

**Required Credentials (.env file):**
```
DATABASE_URL=postgresql://user:password@aws-1-ap-northeast-1.pooler.supabase.com:5432/project_dbname
DATABASE_USER=postgres
DATABASE_PASSWORD=your_password
```

**Application Settings (appsettings.json):**
- Serilog sinks (Console, File)
- Log level (Information in Prod, Debug in Dev)
- Entity Framework Core dialect (Npgsql)

### Build & Run

```bash
# Restore packages
dotnet restore backend/backend.csproj

# Build
dotnet build backend/backend.csproj

# Run (Development mode)
dotnet run --project backend/backend.csproj

# Run (Production mode)
dotnet run --project backend/backend.csproj --configuration Release
```

**App starts on:** http://localhost:5047 (check launchSettings.json for custom ports)

---

## 10. Key Design Decisions & Rationale

1. **Correlation ID Middleware:**
   - Enables request tracing across service boundaries
   - All logs within single HTTP request share same correlation ID
   - Allows troubleshooting entire request lifecycle

2. **Global Exception Middleware:**
   - Centralized error handling (no scattered try-catch)
   - Consistent JSON error response format
   - Includes correlationId in error response for log correlation

3. **Input/Output Logging Pattern:**
   - Method entry logs input parameters (what data was passed)
   - Method exit logs output parameters (what was returned)
   - Enables debugging specific entity operations (e.g., "which product failures did we have?")

4. **Transaction in ProductBusinessService:**
   - Atomic all-or-nothing creation of full product hierarchy
   - Rollback on any error (invalid FK, FK violation, etc.)
   - Prevents orphaned data (images without products, options without products)

5. **Restrict Delete on CustomizationImage FKs:**
   - Prevents cascading delete of CustomizationOptions/Values while images reference them
   - Forced API consumer to handle image cleanup before deleting option/value
   - Product deletion still cascades (cleanup all nested data on product delete)

6. **Scoped Dependency Injection:**
   - Each HTTP request gets new AppDbContext
   - Avoids stale entity references across requests
   - Automatic identity tracking per request

7. **DTO Pattern:**
   - Response DTOs may omit sensitive fields (PasswordHash never in response)
   - Request DTOs define API contract (CreateUserDto != UpdateUserDto)
   - Decouples API response shape from EF domain model

---

## Maintenance Notes

- **Log Retention:** 14 days (rolling daily files)
- **Database:** EF Core migrations managed via `backend/Migrations/` folder
- **ASP.NET Core Version:** Project currently targets .NET 10 (`net10.0`)
- **NuGet Packages:** Keep Serilog, Npgsql, and BCrypt.Net-Next updated
- **Service Scalability:** Current Scoped DI suitable for request volumes up to ~1000 req/s per instance

---

## 11. Order Creation API

### Endpoint

`POST /api/orders/create`

Creates a complete checkout order in one backend transaction.

### Authentication

Requires Bearer JWT with one of these roles:

- `CUSTOMER`
- `ADMIN`
- `SUPER_ADMIN`

The API ignores frontend identity fields. `UserId` is read from the JWT `NameIdentifier` claim. `CustomerOrderId`, totals, payment status, and order status are generated or calculated by backend code.

### Flow

1. `OrderCreationController` receives the request and extracts the JWT user id.
2. `OrderCreationService` opens an EF Core database transaction.
3. `OrderCreationValidatorService` validates user, products, stock, customizations, and prescription compatibility.
4. `CustomerOrderIdGeneratorService` generates `YYMMDDXXXXX`.
5. Backend calculates item totals from product base price and selected customization additional prices.
6. `DiscountService` is called.
7. Existing order services create the order, items, customizations, address, prescription, payment, and status log.
8. Inventory is decremented with a conditional SQL update to avoid concurrent oversell.
9. The transaction commits. On any exception, it rolls back.

### OrderNumberSequences

The `OrderNumberSequences` table stores the daily order number state:

| Column | Description |
|--------|-------------|
| OrderNumberSequencesId | Primary key |
| SequenceDate | Server local date, unique |
| LastSequenceNumber | Last number issued for the date |
| CreatedAt | Row creation timestamp |
| UpdatedAt | Last update timestamp |

`CustomerOrderId` format is `YYMMDDXXXXX`; for example, `26050100001`.

### Discount Status

No discount tables currently exist in the backend. No `Coupons`, `DiscountRules`, `ProductDiscounts`, `CartOffers`, or usage-limit entities are present. `DiscountService` is currently a placeholder and returns:

```json
{
  "discountAmount": 0,
  "finalAmount": "subtotal"
}
```

The frontend may send `couponCode` or `discountCode`, but the backend does not validate expiry, active status, min order amount, usage limits, or eligibility yet. The TODO is to replace the placeholder once discount schema exists.

### Error Contract

Application exceptions are returned by `GlobalExceptionMiddleware`:

```json
{
  "message": "Product not found.",
  "correlationId": "0HN..."
}
```

Expected status codes are `400`, `401`, `403`, `404`, `409`, and `500`.

---

## 12. Order Fetch APIs

### Complete Order Detail

Endpoint: `GET /api/orders/{customerOrderId}`

Roles:

- `CUSTOMER`
- `ADMIN`
- `SUPER_ADMIN`

Behavior:

- Validates `customerOrderId` as `YYMMDDXXXXX`.
- Reads user id and role from JWT claims.
- `CUSTOMER` can fetch only orders whose `Orders.UserId` matches the JWT user id.
- `ADMIN` and `SUPER_ADMIN` can fetch any order.
- Customer payment data is limited to method, status, amount, and created timestamp.
- Admin payment data includes current payment model fields, including transaction id.

Service:

- `IFetchCompleteOrderService`
- `FetchCompleteOrderService`

Uses existing order services for order, items, customizations, addresses, prescriptions, payments, and status logs.

### Admin Order Search

Endpoint: `GET /api/orders`

Roles:

- `ADMIN`
- `SUPER_ADMIN`

Service:

- `IOrderSearchService`
- `OrderSearchService`

Filters:

- `fromCreatedDate`
- `toCreatedDate`
- `orderStatus`
- `paymentStatus`
- `customerOrderId`
- `email`
- `contactNumber`
- `userId`
- `pageNumber`
- `pageSize`

Rules:

- Sorts by `CreatedAt` descending.
- Defaults: `pageNumber=1`, `pageSize=20`.
- Maximum page size is `100`.
- Invalid date range or pagination throws `BadRequestException`.

### Customer My Orders

Endpoint: `GET /api/customer/orders`

Role:

- `CUSTOMER`

Service:

- `ICustomerOrderListService`
- `CustomerOrderListService`

Rules:

- Reads user id from JWT.
- Does not accept user id from query/body.
- Returns only orders owned by the authenticated customer.
- Supports date, order status, payment status, and pagination filters.
- Returns an empty list when no orders match.

Security:

- Customer detail response does not expose transaction id or internal payment id.
- Admin search is unavailable to customers.
- Customer order list is unavailable to admin/super admin by design.
- Services use DTOs and never return EF entities directly.

---

**End of Documentation**
**Generated: April 9, 2026**
