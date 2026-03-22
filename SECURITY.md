# Security & Validation Guidelines

## Input Validation Strategy

All API endpoints validate incoming requests using **FluentValidation 11.x**. 
Validators are registered automatically via `AddValidatorsFromAssembly()` 
in the dependency injection pipeline and are enforced at the controller level.

### Validation Architecture

Validators inherit from `AbstractValidator<T>` and are located in:
```
ExpenseManager.Application/Validators/
```

---

## Authentication & Password Security

### RegisterRequestValidator

- **Email**: Required, valid format, max 256 chars
  - Uses `EmailAddress()` for RFC 5322 compliance
  - Prevents email spoofing and invalid formats
- **Password**: Required, enforces strong password policy
  - Minimum 8 characters
  - Maximum 128 characters (prevents excessively long inputs)
  - Must contain uppercase letter (A-Z)
  - Must contain lowercase letter (a-z)
  - Must contain digit (0-9)
  - Must contain special character (!@#$%^&\*()\_+\-=[]{}';:"\\|,.<>?)
  - **Rationale**: Multi-character set requirement prevents brute force 
    attacks and dictionary-based exploits
- **UserName**: Required, 3-50 alphanumeric characters
  - Matches pattern: `^[a-zA-Z0-9]+$`
  - Prevents injection attacks via username field
  - Restricts to safe characters only

### LoginRequestValidator

- **Email**: Required, valid email format
- **Password**: Basic validation on login
  - Required (must not be empty)
  - Maximum 128 characters
  - **Note**: Does NOT enforce strong password policy on login to avoid 
    locking out legitimate users with valid passwords. Strength is enforced 
    at registration time only.

---

## Financial Data Validation

### CreateExpenseValidator & UpdateExpenseValidator

- **CategoryId**: Required, non-empty Guid
  - Enforces referential integrity at the API level
  - Prevents categorization errors

- **Amount**: Required fintech-precision decimal
  - Must be > 0 (prevents zero or negative amounts)
  - Maximum: 9999999999999999.9999 (18,4 precision)
  - **Rationale**: Fintech standard precision (18,4) for currency amounts 
    to prevent rounding errors in financial calculations. 
    Matches database column definition `numeric(18,4)`

- **Currency**: Required, exactly 3 uppercase ISO 4217 code
  - Examples: USD, EUR, GBP, LKR
  - Matches pattern: `^[A-Z]+$` (uppercase letters only)
  - Uppercase enforcement prevents case-mismatch errors
  - Exact 3-char length requirement validates ISO 4217 standard

- **ExchangeRate**: Optional, but validated if provided
  - When present, must be > 0
  - Prevents invalid conversion rates and data corruption
  - Reserved for future multi-currency conversion features

- **Description**: Optional, max 500 chars when provided
  - Prevents database overflow
  - Matches database column definition `character varying(500)`

- **Date**: Required, cannot be in the future
  - Uses `DateTimeOffset.UtcNow` for UTC comparison
  - Prevents future-dated expenses (audit trail integrity)

> **Note**: UserId is never included in expense DTOs or validators.
> It is always extracted from JWT claims via `CurrentUserId` 
> in `BaseController`. Accepting UserId from the request body 
> would introduce IDOR vulnerabilities.

---

## Category Validation

### CreateCategoryValidator

- **Name**: Required, 2-100 chars
  - Minimum length prevents empty categories
  - Maximum prevents resource exhaustion
  - Matches database column definition `character varying(100)`
- **Description**: Optional, max 250 chars
  - Matches database column definition `character varying(250)`

> **Note**: `UserId` and `IsPredefined` are never included in 
> `CreateCategoryDto` or its validator.
> - `UserId` is always set from JWT claims in the service layer
> - `IsPredefined` is always set to `false` in the service layer
> - Accepting these from the client would allow privilege escalation

---

## Date Filter Validation

### ExpenseFilterDtoValidator

- **FromDate / ToDate**: Optional `DateOnly` values
  - When both provided, `FromDate` must be ≤ `ToDate`
  - Prevents invalid date ranges in expense filtering and CSV export
  - Manually triggered in controller (query params bypass automatic validation)

---

## Why FluentValidation?

1. **Declarative**: Validation rules co-locate with DTOs for maintainability
2. **Composable**: Easy to extend or modify validation chains
3. **Testable**: Validators can be unit-tested independently
4. **ASP.NET Integrated**: Automatic ModelState binding error response formatting
5. **Security-focused**: Regex patterns prevent injection attacks

---

## Error Response Format

Invalid requests return HTTP 400 with structured error details:
```json
{
  "errors": {
    "Email": ["Email must be a valid email address."],
    "Password": ["Password must contain at least one uppercase letter."]
  }
}
```

---

## Validation Order & Performance

- Validators run **before** controller action methods
- Simple rules (required, length) execute first
- Complex rules (regex, custom logic) execute after basic checks
- Validation fails fast on first error per property (by default)

---

## Roles and Permissions

| Resource | User | Admin |
|----------|------|-------|
| Own expenses | ✅ CRUD | ✅ CRUD |
| Other's expenses | ❌ | ❌ |
| Predefined categories | ✅ Read | ✅ Read |
| Own categories | ✅ CRUD | ✅ CRUD |
| Other's categories | ❌ | ❌ |
| All users list | ❌ | ✅ Read |
| Own profile (/me) | ✅ Read | ✅ Read |
| Any user profile | ❌ | ✅ Read |

---

## Security Design Decisions

### Why UserId is Never in Request DTOs
- Accepting UserId from request body introduces IDOR vulnerabilities
- Users could modify other users' data by changing the UserId
- UserId always extracted from JWT claims via `CurrentUserId` in `BaseController`

### Why IsPredefined is Never in Request DTOs
- Accepting IsPredefined from client allows privilege escalation
- Users could create system-level categories
- Always set to `false` in service layer for user-created categories

### Why UserEmail is Not in ExpenseResponseDto
- Unnecessary data exposure in every expense response
- User already knows their own email
- Reduces data leakage surface

---

## Future Enhancements

- Refresh token support with token revocation
- Rate limiting per user/IP to prevent brute force attacks
- Async validators for database-level checks (e.g., email uniqueness)
- Localized error messages for multi-language support
- Idempotency keys for financial operations