# ExpenseManager API

A robust ASP.NET Core 8 Web API for managing personal expenses, built with Clean Architecture principles across layered projects.

The Next.js frontend (`expense-manager-web`) lives in the same workspace and proxies to this API at `/api/v1`.

## Quick start (terminal)

From the `ExpenseManager.API` folder (solution root):

```powershell
cd ExpenseManager.API
dotnet restore
dotnet ef database update --project ExpenseManager.Infrastructure --startup-project ExpenseManager.API
dotnet run --project ExpenseManager.API/ExpenseManager.API.csproj
```

Default URLs (see `ExpenseManager.API/Properties/launchSettings.json`):

| Profile | URL |
|---------|-----|
| HTTPS | `https://localhost:7183` |
| HTTP | `http://localhost:5216` |
| Swagger | `https://localhost:7183/swagger` |

Set `API_PROXY_TARGET=https://localhost:7183` in the web app's `.env.local` so the frontend can reach the API.

> **EF Core tools:** If `dotnet ef` is not found, install once:
> ```powershell
> dotnet tool install --global dotnet-ef
> ```

## Project structure

```
ExpenseManager.API.sln
├── ExpenseManager.Domain          → Entities, Enums
├── ExpenseManager.Application     → DTOs, Interfaces, Services, Validators, AutoMapper
├── ExpenseManager.Infrastructure  → DbContext, Repositories, EF Migrations, JWT, BCrypt, CSV, OCR
└── ExpenseManager.API             → Controllers, Middleware, Program.cs
```

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL](https://www.postgresql.org/download/)
- [dotnet-ef](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) global tool (for migrations)
- Visual Studio 2022 or VS Code (optional)

## Setup

### 1. Restore dependencies

```powershell
dotnet restore
```

### 2. Configure local secrets

Create `ExpenseManager.API/appsettings.Development.json` (gitignored — **never commit secrets**):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=ExpenseManagerDb;Username=postgres;Password=your_password"
  },
  "JwtSettings": {
    "Secret": "your-super-secret-key-minimum-32-characters-long"
  },
  "AzureBlob": {
    "ConnectionString": "your-azure-storage-connection-string",
    "Container": "receipts"
  },
  "AzureDocumentIntelligence": {
    "Endpoint": "https://your-resource.cognitiveservices.azure.com/",
    "ApiKey": "your-document-intelligence-key"
  }
}
```

`appsettings.json` holds non-secret defaults only (CORS, empty placeholders).

| Setting | Required for | Notes |
|---------|--------------|-------|
| `DefaultConnection` | All features | PostgreSQL connection string |
| `JwtSettings:Secret` | Auth | Min 32 characters |
| `AzureBlob:ConnectionString` | Receipt upload | Azure Blob Storage |
| `AzureDocumentIntelligence` | Real OCR | Optional locally — mock OCR runs when `Endpoint` is empty |

### 3. Apply database migrations

Run from the solution root (`ExpenseManager.API`):

```powershell
dotnet ef database update --project ExpenseManager.Infrastructure --startup-project ExpenseManager.API
```

This creates/updates `ExpenseManagerDb` and seeds predefined categories (Food & Dining, Transport, Health & Medical).

### 4. Run the API

```powershell
dotnet run --project ExpenseManager.API/ExpenseManager.API.csproj
```

Or press **F5** in Visual Studio.

Watch the terminal for the listening URL, then open Swagger or point the frontend proxy at it.

## Database migrations

**Apply pending migrations:**

```powershell
dotnet ef database update --project ExpenseManager.Infrastructure --startup-project ExpenseManager.API
```

**Add a new migration** (after entity changes):

```powershell
dotnet ef migrations add MigrationName --project ExpenseManager.Infrastructure --startup-project ExpenseManager.API
```

**Remove the last migration** (if not applied):

```powershell
dotnet ef migrations remove --project ExpenseManager.Infrastructure --startup-project ExpenseManager.API
```

## API documentation

- **Swagger UI** (Development only): `https://localhost:7183/swagger`
- **Endpoint contract**: [`API_DOC.md`](./API_DOC.md) — keep in sync with `expense-manager-web/src/types/index.ts`
- **Architecture & security**: [`ARCHITECTURE.md`](./ARCHITECTURE.md), [`SECURITY.md`](./SECURITY.md)
- **AI / contributor guide**: [`AGENTS.md`](./AGENTS.md)

Base route pattern: `api/v1/[controller]` (singular), e.g. `/api/v1/expense`, `/api/v1/receipt`.

## Authentication

JWT Bearer authentication:

1. Register: `POST /api/v1/auth/register`
2. Login: `POST /api/v1/auth/login` — copy the token
3. In Swagger, click **Authorize** and enter: `Bearer {your_token}`

## Creating an admin user

Registered users default to the `User` role. To promote to Admin:

```sql
UPDATE "Users" SET "Role" = 'Admin' WHERE "Email" = 'admin@example.com';
```

Log in again to receive a token with the Admin role.

## Frontend integration

The web app rewrites `/api/v1/*` to the backend. In `expense-manager-web/.env.local`:

```
API_PROXY_TARGET=https://localhost:7183
```

CORS allows `http://localhost:3000` via `Cors:AllowedOrigins` in `appsettings.json`.

## Environment files

| File | Purpose | Committed? |
|------|---------|------------|
| `appsettings.json` | Base config, no secrets | Yes |
| `appsettings.Development.json` | Local DB, JWT, Azure keys | No (gitignored) |

## Tech stack

| Layer | Technologies |
|-------|--------------|
| API | ASP.NET Core 8, Serilog, Swagger, JWT Bearer |
| Application | AutoMapper, FluentValidation |
| Infrastructure | EF Core, Npgsql, BCrypt.Net, CsvHelper, Azure Blob, Azure Document Intelligence |
| Database | PostgreSQL |
