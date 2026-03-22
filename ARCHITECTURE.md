# ExpenseManager API — Architecture Documentation

## Authentication & Authorization

### JWT Token Strategy
- Single access token issued on login
- Token expiry: 60 minutes (configurable via JwtSettings:ExpiryMinutes)
- No refresh token implemented in current version
- Users must re-authenticate after token expiry
- JWT secret stored in appsettings.Development.json only (gitignored)

### Future Consideration
- Refresh token implementation recommended for production
- Sliding expiration or refresh token rotation for better UX
- Token revocation/blacklisting not implemented in current version

### Role-Based Authorization
- Two roles: User and Admin
- Role embedded in JWT claims on login
- Admin: full access to all users and data
- User: access to own data only
- Role enforcement at controller level via [Authorize(Roles = "Admin")]

---

## Transaction Strategy

### Current Approach
- Single SaveChangesAsync() per operation
- EF Core wraps each SaveChangesAsync() in an implicit transaction
- Sufficient for all current single-entity CRUD operations

### Explicit Transaction Pattern (available for future use)
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

### Idempotency
- Not implemented in current version
- Recommended for future payment or webhook integrations
- Current CRUD operations are atomic via implicit transactions

---

## Multi-Currency Support

Each expense stores its original `Amount`, `Currency` (ISO 4217 code
e.g. USD, LKR), and an optional `ExchangeRate` field.

Full multi-currency conversion is not implemented in this version.
The `ExchangeRate` field is nullable and reserved for future implementation
where amounts can be normalized to a base currency for cross-currency
reporting and summaries.

---

## Delete Strategy

This application uses soft delete across all entities via the `IsDeleted`
flag in `BaseEntity`. Hard deletion is intentionally avoided to preserve
financial records and maintain audit trails.

### Why Soft Delete for Fintech

Financial records must never be permanently deleted. Regulatory compliance
and dispute resolution require complete transaction history. Deleting an
expense or user account should never result in data loss.

### Delete Behavior Configuration

All foreign key relationships use `Restrict` delete behavior:
```csharp
// Expense → User
.OnDelete(DeleteBehavior.Restrict);

// Expense → Category
.OnDelete(DeleteBehavior.Restrict);
```

**Reasoning:**

- `Cascade` was intentionally avoided — deleting a user or category
  should never wipe financial records
- `Restrict` enforces data integrity at the database level
- All deletions are handled as soft deletes at the application level

### Soft Delete Flow
```
DELETE request received
        ↓
Service sets IsDeleted = true + DeletedAt = UtcNow
        ↓
SaveChangesAsync persists the change
        ↓
HasQueryFilter excludes record from all future queries
        ↓
Financial record preserved in database ✅
```

### Global Query Filter

EF Core `HasQueryFilter` is applied in `BaseEntityConfiguration` to
automatically exclude soft deleted records from all queries:
```csharp
builder.HasQueryFilter(e => !e.IsDeleted);
```

To access deleted records (admin only):
```csharp
_context.Expenses.IgnoreQueryFilters().ToListAsync();
```

### Category → User Relationship
`SetNull` was intentionally avoided for the Category → User foreign key.
Setting `UserId` to null on soft delete would make user-defined categories
appear as predefined categories, corrupting category ownership logic.
`Restrict` is used instead, relying on soft delete to hide records.

---

## Layer Separation

### Domain Layer
- Entity definitions only
- No business logic
- No external dependencies

### Application Layer
- Business logic in services
- DTOs for data transfer
- Validators using FluentValidation
- Interfaces for repositories and services
- No direct DB access

### Infrastructure Layer
- EF Core DbContext
- Repository implementations
- JWT token generation
- No business logic

### API Layer
- Controllers only handle HTTP concerns
- UserId always extracted from JWT claims via BaseController
- Never trust UserId from request body
- GlobalExceptionMiddleware for centralized error handling

---

## Security Considerations

### JWT
- Secret minimum 32 characters
- Stored only in appsettings.Development.json (gitignored)
- Issuer and Audience validated on every request

### CORS
- Allowed origins read from configuration
- Never hardcoded in code
- Currently allows http://localhost:3000 (Next.js frontend)

### Data Security
- UserId never accepted from request body
- IsPredefined never trusted from client
- UserEmail removed from ExpenseResponseDto
- IDOR protection via ownership checks in service layer

---

## Validation Strategy
- FluentValidation for request body DTOs
- Manual validation trigger for query parameter DTOs (ExpenseFilterDto)
- InvalidModelStateResponseFactory for automatic model validation logging
- FromDate must be less than or equal to ToDate in filters
- Date filtering uses DateOnly for cleaner input (no time component)
- Date ranges normalized to start/end of day in repository layer

---

## Export Strategy
- CSV export using CsvHelper
- Filtered by same ExpenseFilterDto as expense listing
- Returns byte[] for file download
- Column headers derived from property names
- No Excel/column width support in current version

---

## Logging Strategy
- Serilog for structured logging
- Console and file sinks
- Rolling daily log files in logs/ directory
- Validation failures logged as Warning
- Exceptions handled and logged by GlobalExceptionMiddleware
- Microsoft and System namespaces set to Warning level

---

## Known Limitations & Future Improvements
- No refresh token support
- No token revocation/blacklisting
- No idempotency keys
- No pagination on list endpoints
- No rate limiting
- No Excel export (CSV only)
- No full multi-currency conversion (ExchangeRate reserved for future)
- CORS limited to localhost (needs update for production)
- JWT secret needs secrets manager for production deployment