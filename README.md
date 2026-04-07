# Project Eyewear - .NET Backend

ASP.NET Core Web API with Entity Framework Core and Supabase PostgreSQL.

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [EF Core CLI](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)

```bash
dotnet tool install --global dotnet-ef
```

## Project Structure

```
backend/
├── Controllers/            # API controllers
├── Data/                   # DbContext
├── DTOs/                   # Data Transfer Objects
│   └── UserDtos/           # User-specific DTOs (Create, Update, Response)
├── Migrations/             # EF Core migrations
├── Models/                 # Entity models
├── Services/               # Business logic
│   └── UserService/        # User service (interface + implementation)
├── openapi.json            # Static OpenAPI spec (for frontend integration)
├── Program.cs              # App entry point
├── appsettings.json        # Production config (no credentials)
└── appsettings.Development.json  # Dev config (with DB connection string)
```

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/ajitgitcontact/project-eyewear-dotnet.git
cd project-eyewear-dotnet
```

### 2. Restore dependencies

```bash
cd backend
dotnet restore
```

### 3. Configure the database

Edit `backend/appsettings.Development.json` and set your Supabase connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=<POOLER_HOST>;Port=5432;Database=postgres;Username=<USERNAME>;Password=<PASSWORD>;SSL Mode=Require;Trust Server Certificate=true"
  }
}
```

**Supabase connection tips:**
- Go to your Supabase project → **Settings → Database → Connection String**
- If you're on an **IPv4 network**, use the **Session Pooler** connection (not direct)
- The pooler host looks like: `aws-<region>.pooler.supabase.com`
- The pooler username includes your project ref: `postgres.<project-ref>`

### 4. Apply database migrations

```bash
dotnet ef database update
```

This creates the `Users` table and migration history in your Supabase database.

### 5. Run the application

```bash
dotnet run
```

The API starts at: `http://localhost:5047`

## API Endpoints

### Swagger UI

Open [http://localhost:5047/swagger](http://localhost:5047/swagger) for interactive API docs.

### OpenAPI Spec

A static `openapi.json` file is available at `backend/openapi.json` for frontend integration, Postman import, or code generation.

To regenerate it while the server is running:

```bash
curl -s http://localhost:5047/openapi/v1.json -o backend/openapi.json
```

### Users API

| Method | Endpoint                  | Description        |
|--------|---------------------------|--------------------|
| GET    | `/api/users`              | Get all users      |
| GET    | `/api/users/{id}`         | Get user by ID     |
| GET    | `/api/users/email/{email}`| Get user by email  |
| POST   | `/api/users`              | Create a new user  |
| PUT    | `/api/users/{id}`         | Update a user      |
| DELETE | `/api/users/{id}`         | Delete a user      |

### DB Test

| Method | Endpoint       | Description              |
|--------|----------------|--------------------------|
| GET    | `/api/dbtest`  | Test database connection |

## Tech Stack

- **.NET 9** — ASP.NET Core Web API
- **Entity Framework Core 9** — ORM
- **Npgsql** — PostgreSQL provider
- **Supabase** — Hosted PostgreSQL database
- **Swagger / OpenAPI** — API documentation
