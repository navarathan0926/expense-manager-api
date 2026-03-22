# ExpenseManager API

A robust ASP.NET Core 8 Web API for managing personal expenses, 
built with Clean Architecture principles across layered projects.

## 🏗️ Project Structure
```
ExpenseManager.sln
├── ExpenseManager.Domain          → Entities, Enums
├── ExpenseManager.Application     → DTOs, Interfaces, Services, Validators, AutoMapper
├── ExpenseManager.Infrastructure  → DbContext, Repositories, EF Migrations, JWT, BCrypt, CSV
└── ExpenseManager.API             → Controllers, Middleware, Program.cs
```

## 🚀 Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL](https://www.postgresql.org/download/)
- Visual Studio 2022 or VS Code

## ⚙️ Setup Instructions

### 1. Clone the repository
```bash
git clone <repository-url>
cd ExpenseManager
```

### 2. Restore dependencies
```bash
dotnet restore
```

### 3. Configure environment

Create `appsettings.Development.json` in `ExpenseManager.API` 
(this file is gitignored — never commit secrets):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=ExpenseManagerDb;Username=postgres;Password=your_password"
  },
  "JwtSettings": {
    "Secret": "your-super-secret-key-minimum-32-characters-long"
  }
}
```

Base configuration is in `appsettings.json` — no secrets there.

### 4. Apply database migrations

Migrations are in `ExpenseManager.Infrastructure`. 
Run from the solution root:
```bash
dotnet ef database update \
  --project ExpenseManager.Infrastructure \
  --startup-project ExpenseManager.API
```

This will:
- Create the `ExpenseManagerDb` database automatically
- Apply all migrations
- Seed predefined categories (Food & Dining, Transport, Health & Medical)

### 5. Run the application
```bash
dotnet run --project ExpenseManager.API/ExpenseManager.API.csproj
```

Or press `F5` in Visual Studio.

## 📖 API Documentation

Swagger UI is available in development mode only:
```
https://localhost:{port}/swagger
```

Check the terminal output for the exact port after running.

## 🔐 Authentication

This API uses JWT Bearer authentication.

1. Register a user: `POST api/v1/auth/register`
2. Login: `POST api/v1/auth/login` — copy the token from response
3. In Swagger, click **Authorize** and enter: `Bearer {your_token}`

## 👤 Creating an Admin User

All registered users default to `User` role. To create an Admin:

1. Register a user normally
2. Update the role manually in PostgreSQL:
```sql
UPDATE "Users" SET "Role" = 'Admin' WHERE "Email" = 'admin@example.com';
```

3. Login again to get a new token with Admin role

## 🗂️ Adding Migrations

When making entity changes, always specify both projects:
```bash
dotnet ef migrations add MigrationName \
  --project ExpenseManager.Infrastructure \
  --startup-project ExpenseManager.API
```

## 🔧 Environment Configuration

| File | Purpose | Committed? |
|---|---|---|
| `appsettings.json` | Base config, no secrets | ✅ Yes |
| `appsettings.Development.json` | Local secrets, connection string | ❌ No |

## 📦 Tech Stack

| Layer | Technologies |
|---|---|
| API | ASP.NET Core 8, Serilog, Swagger, JWT Bearer |
| Application | AutoMapper, FluentValidation |
| Infrastructure | EF Core, Npgsql, BCrypt.Net, CsvHelper |
| Database | PostgreSQL |