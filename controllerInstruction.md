# ExpenseManager — Phase 5 Controller Implementation Instructions

## Project Context
- ASP.NET Core 8 Web API
- Base route: `api/v1/[controller]`
- JWT Authentication with role-based authorization (User/Admin)
- Global exception middleware already implemented
- All services and repositories from Phase 4 are complete

## General Controller Rules
- All controllers inherit from `BaseController` (not ControllerBase directly)
- Use `ActionResult<T>` when returning data
- Use `IActionResult` when returning no data (Delete, void)
- UserId and IsAdmin always from `BaseController` properties
- Never extract UserId from request body

---

## BaseController
**Location:** `ExpenseManager.API/Controllers/BaseController.cs`
```csharp
[ApiController]
[Route("api/v1/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    protected bool IsAdmin =>
        User.IsInRole("Admin");
}
```

- All controllers inherit from `BaseController`
- `BaseController` has `[ApiController]` and `[Route]` attributes
- Individual controllers do NOT repeat these attributes
- `CurrentUserId` and `IsAdmin` available in all controllers

---

## Program.cs Setup

### 1. Serilog
```csharp
builder.Host.UseSerilog((context, config) =>
{
    config
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console()
        .WriteTo.File("logs/expense-manager-.txt", 
            rollingInterval: RollingInterval.Day);
});
```

### 2. JWT Authentication
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    builder.Configuration["JwtSettings:Secret"]!))
        };
    });
```

### 3. Swagger with JWT Support
```csharp
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "ExpenseManager API", 
        Version = "v1" 
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer {token}'"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
```

### 4. CORS for Next.js
**Read from configuration — never hardcoded:**
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("NextJsPolicy", policy =>
    {
        var allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>()!;

        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```

### 5. Dependency Injection
```csharp
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
```

### 6. Middleware Pipeline Order
```csharp
// Order is critical — do not change
app.UseMiddleware<GlobalExceptionMiddleware>(); // must be first
app.UseHttpsRedirection();
app.UseCors("NextJsPolicy");
app.UseAuthentication();  // must come before UseAuthorization
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
```

---

## appsettings.json
```json
{
  "JwtSettings": {
    "Issuer": "ExpenseManagerAPI",
    "Audience": "ExpenseManagerClient",
    "ExpiryMinutes": 60
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000"]
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  }
}
```

## appsettings.Development.json (gitignored — never commit)
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

---

## Controllers

### 1. AuthController
**Route:** `api/v1/auth`
**No [Authorize] — public endpoints**
```
POST api/v1/auth/register
- Body: RegisterRequestDto
- Validate with FluentValidation
- Returns: ActionResult<AuthResponseDto>
- Response: 201 Created

POST api/v1/auth/login
- Body: LoginRequestDto
- Validate with FluentValidation
- Returns: ActionResult<AuthResponseDto>
- Response: 200 Ok
```

---

### 2. ExpenseController
**Route:** `api/v1/expenses`
**All endpoints require [Authorize]**
**Use CurrentUserId from BaseController**
```
GET api/v1/expenses
- [Authorize]
- Query params: FromDate?, ToDate?, CategoryId?,
  MinAmount?, MaxAmount?, Currency?
- Map query params to ExpenseFilterDto
- Call _expenseService.GetAllAsync(CurrentUserId, filters)
- Returns: ActionResult<IEnumerable<ExpenseResponseDto>>
- Response: 200 Ok

GET api/v1/expenses/{id}
- [Authorize]
- Call _expenseService.GetByIdAsync(id, CurrentUserId)
- Returns: ActionResult<ExpenseResponseDto>
- Response: 200 Ok

POST api/v1/expenses
- [Authorize]
- Body: CreateExpenseDto
- Call _expenseService.CreateAsync(dto, CurrentUserId)
- Returns: ActionResult<ExpenseResponseDto>
- Response: 201 Created

PUT api/v1/expenses/{id}
- [Authorize]
- Body: UpdateExpenseDto
- Call _expenseService.UpdateAsync(id, dto, CurrentUserId)
- Returns: ActionResult<ExpenseResponseDto>
- Response: 200 Ok

DELETE api/v1/expenses/{id}
- [Authorize]
- Call _expenseService.DeleteAsync(id, CurrentUserId)
- Returns: IActionResult
- Response: 204 NoContent
```

---

### 3. CategoryController
**Route:** `api/v1/categories`
```
GET api/v1/categories
- No [Authorize] required
- If user is authenticated → return predefined + own categories
  using CurrentUserId
- If not authenticated → return predefined categories only
  using Guid.Empty as userId
- Returns: ActionResult<IEnumerable<CategoryResponseDto>>
- Response: 200 Ok

POST api/v1/categories
- [Authorize]
- Body: CreateCategoryDto
- Call _categoryService.CreateAsync(dto, CurrentUserId)
- Returns: ActionResult<CategoryResponseDto>
- Response: 201 Created

DELETE api/v1/categories/{id}
- [Authorize]
- Call _categoryService.DeleteAsync(id, CurrentUserId)
- Returns: IActionResult
- Response: 204 NoContent
```

---

### 4. ReportController
**Route:** `api/v1/reports`
**All endpoints require [Authorize]**
**Use CurrentUserId from BaseController**
```
GET api/v1/reports/monthly-summary
- [Authorize]
- Query params: year (int), month (int)
- Call _reportService.GetMonthlySummaryAsync(CurrentUserId, year, month)
- Returns: ActionResult<MonthlySummaryDto>
- Response: 200 Ok

GET api/v1/reports/category-breakdown
- [Authorize]
- Query params: year (int), month (int)
- Call _reportService.GetCategoryBreakdownAsync(CurrentUserId, year, month)
- Returns: ActionResult<IEnumerable<CategoryBreakdownDto>>
- Response: 200 Ok
```

---

### 5. ExportController
**Route:** `api/v1/export`
**All endpoints require [Authorize]**
**Use CurrentUserId from BaseController**
```
GET api/v1/export/csv
- [Authorize]
- Query params: FromDate?, ToDate?, CategoryId?,
  MinAmount?, MaxAmount?, Currency?
- Map query params to ExpenseFilterDto
- Call _csvExportService.ExportExpensesAsync(CurrentUserId, filters)
- Returns file download:
  return File(bytes, "text/csv", "expenses.csv");
- Response: 200 Ok with file
```

---

### 6. UserController
**Route:** `api/v1/users`
```
GET api/v1/users
- [Authorize(Roles = "Admin")] — Admin only
- Call _userService.GetAllUsersAsync()
- Returns: ActionResult<IEnumerable<UserResponseDto>>
- Response: 200 Ok

GET api/v1/users/{id}
- [Authorize]
- Call _userService.GetByIdAsync(id, CurrentUserId, IsAdmin)
- Returns: ActionResult<UserResponseDto>
- Response: 200 Ok
```

---

## Transaction Handling Note
- Each service method uses a single SaveChangesAsync() call
- EF Core wraps each SaveChangesAsync() in an implicit transaction
- This is sufficient for all current single-entity operations
- Multi-step operations requiring explicit transactions would use:
```csharp
  await using var transaction = 
      await _context.Database.BeginTransactionAsync();
  try
  {
      // multiple operations
      await transaction.CommitAsync();
  }
  catch
  {
      await transaction.RollbackAsync();
      throw;
  }
```
- Document this in ARCHITECTURE.md for assessment

---

## Important Rules
- `GlobalExceptionMiddleware` must be first in pipeline
- `UseAuthentication` must come before `UseAuthorization`
- `UseCors` must come before `UseAuthentication`
- CORS origins must come from configuration, never hardcoded
- JWT secret must be in `appsettings.Development.json` only
- `CurrentUserId` and `IsAdmin` always from `BaseController`
- Never trust UserId from request body
- Register returns 201 Created
- Delete returns 204 NoContent
- CSV export returns `File()` not `Ok()`
- Namespace convention: `ExpenseManager.API.Controllers`