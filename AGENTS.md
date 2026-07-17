# Expense Manager API — AI Development Guide

Standalone ASP.NET Core 8 Web API repository with Clean Architecture and PostgreSQL.

The Next.js frontend lives in a **separate repository** (`expense-manager-web`). When API contracts change, update `API_DOC.md` so the frontend team can sync types.

## Solution Structure

```
ExpenseManager.API.sln
├── ExpenseManager.Domain/          # Entities, enums (no dependencies)
├── ExpenseManager.Application/       # DTOs, services, validators, interfaces
├── ExpenseManager.Infrastructure/    # DbContext, repos, JWT, BCrypt, CSV
└── ExpenseManager.API/               # Controllers, middleware, Program.cs
```

## Clean Architecture Rules

### What Goes Where

```
Domain        → User, Expense, Category, BaseEntity
Application   → ExpenseService, CreateExpenseDto, CreateExpenseValidator, IExpenseRepository
Infrastructure → ExpenseRepository, ExpenseManagerDbContext, JwtTokenGenerator
API           → ExpenseController, GlobalExceptionMiddleware, Program.cs
```

### Dependency Rules

- Domain: zero external package references
- Application: references Domain only; no EF Core, no HTTP packages
- Infrastructure: references Application + Domain; implements repository interfaces
- API: references Application + Infrastructure; thin controllers only

## Security (Non-Negotiable)

- `CurrentUserId` from `BaseController` — **never** accept UserId in request body
- Ownership checks in **service layer** before read/update/delete
- Soft delete only (`IsDeleted = true`) — financial records are never hard-deleted
- `decimal` for amounts; never `float`/`double`
- JWT secret in `appsettings.Development.json` only (gitignored)
- BCrypt for password hashing — never store plain text

## Controller Pattern

```csharp
[Authorize]
public class ExpenseController : BaseController
{
    [HttpPost]
    public async Task<ActionResult<ExpenseResponseDto>> Create([FromBody] CreateExpenseDto dto)
    {
        var created = await _expenseService.CreateAsync(dto, CurrentUserId);
        return StatusCode(StatusCodes.Status201Created, created);
    }
}
```

- Inherit `BaseController` (provides `[Route("api/v1/[controller]")]`)
- Do NOT repeat `[ApiController]` or `[Route]` on child controllers
- Singular route names: `ExpenseController` → `/api/v1/expense`

## Service Pattern

```csharp
public async Task<ExpenseResponseDto> CreateAsync(CreateExpenseDto dto, Guid userId)
{
    // Validate ownership, existence, business rules
    var expense = _mapper.Map<Expense>(dto);
    expense.UserId = userId;  // Always from parameter, never DTO
    await _expenseRepository.AddAsync(expense);
    await _expenseRepository.SaveChangesAsync();
    return _mapper.Map<ExpenseResponseDto>(await _expenseRepository.GetByIdWithDetailsAsync(expense.Id));
}
```

## Exception Handling

| Exception | HTTP Status |
|-----------|-------------|
| `NotFoundException` | 404 |
| `ConflictException` | 409 |
| `UnauthorizedException` | 401 |
| All others | 500 |

Thrown in services, caught by `GlobalExceptionMiddleware`.

## Adding a New Feature Checklist

1. **Domain** — add/modify entity in `ExpenseManager.Domain/Entities/`
2. **Application** — DTO, validator, service interface + implementation
3. **Infrastructure** — repository methods, EF configuration, migration
4. **API** — controller endpoint
5. **Migration** — `dotnet ef migrations add {Name} --project ExpenseManager.Infrastructure --startup-project ExpenseManager.API`
6. **API contract** — update `API_DOC.md`; notify frontend repo to sync `src/types/index.ts`

## Key Documentation

| File | Use when |
|------|----------|
| `ARCHITECTURE.md` | Understanding auth, soft delete, logging, CORS |
| `API_DOC.md` | Endpoint contracts and response shapes |
| `SECURITY.md` | Validation rules and security policies |
| `instructions.md` | Service/repository interfaces and business rules |
| `controllerInstruction.md` | Controller setup and Program.cs config |
| `rules.md` | NuGet package placement decisions |
| `DATABASE.md` | Schema details |

## Running

```bash
dotnet restore
dotnet ef database update --project ExpenseManager.Infrastructure --startup-project ExpenseManager.API
dotnet run --project ExpenseManager.API/ExpenseManager.API.csproj
# Swagger: https://localhost:{port}/swagger
```

## Roles

- **User** — CRUD own expenses/categories; read predefined categories
- **Admin** — above + list/manage all users
- Neither role can access another user's expenses or categories
