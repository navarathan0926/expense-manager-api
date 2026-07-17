# ExpenseManager — DTO Cleanup & User Endpoint Refactor

## 1. Remove UserId from CreateExpenseDto
**Location:** `ExpenseManager.Application/DTOs/CreateExpenseDto.cs`

Remove `UserId` property entirely:
```csharp
public class CreateExpenseDto
{
    public Guid CategoryId { get; set; }
    public decimal Amount { get; set; }
    public required string Currency { get; set; }
    public decimal? ExchangeRate { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset Date { get; set; }
}
```

Reason: UserId is always set from JWT claims in the service layer.
Exposing it in the DTO allows potential IDOR attacks.

Also update MappingProfile.cs:
- Remove `.ForMember(dest => dest.UserId, opt => opt.Ignore())`
  from `CreateExpenseDto → Expense` mapping since UserId
  no longer exists in the DTO

## 2. Remove UserId and IsPredefined from CreateCategoryDto
**Location:** `ExpenseManager.Application/DTOs/CreateCategoryDto.cs`
```csharp
public class CreateCategoryDto
{
    public required string Name { get; set; }
    public string? Description { get; set; }
}
```

Reason: 
- UserId always comes from JWT claims
- IsPredefined is always false for user-created categories
  and must be set in service layer, never trusted from client

Also update MappingProfile.cs:
- Remove `.ForMember(dest => dest.UserId, opt => opt.Ignore())`
  from `CreateCategoryDto → Category` mapping
  
Also update CategoryService.CreateAsync:
- Set category.IsPredefined = false explicitly after mapping
- Set category.UserId = userId from JWT claims parameter

## 3. User Endpoints Refactor
**Location:** `ExpenseManager.API/Controllers/UserController.cs`

### Current (wrong):
```
GET api/v1/users/{id}  → [Authorize] → any authenticated user
```

### Replace with two endpoints:

**Endpoint 1 — GET api/v1/users/me**
- [Authorize] — any authenticated user
- Gets own profile using CurrentUserId
- No id parameter needed
- Calls: _userService.GetByIdAsync(CurrentUserId)

**Endpoint 2 — GET api/v1/users/{id}**
- [Authorize(Roles = "Admin")] — Admin only
- Gets any user by id
- Calls: _userService.GetByIdAsync(id)

### Updated UserController:
```csharp
[Authorize]
[HttpGet("me")]
public async Task<ActionResult<UserResponseDto>> GetMe()
{
    var user = await _userService.GetByIdAsync(CurrentUserId);
    return Ok(user);
}

[Authorize(Roles = "Admin")]
[HttpGet("{id}")]
public async Task<ActionResult<UserResponseDto>> GetById(Guid id)
{
    var user = await _userService.GetByIdAsync(id);
    return Ok(user);
}
```

## 4. Simplify IUserService.GetByIdAsync
**Location:** `ExpenseManager.Application/Interfaces/IUserService.cs`

Change from:
```csharp
Task<UserResponseDto> GetByIdAsync(Guid id, Guid currentUserId, bool isAdmin);
```

To:
```csharp
Task<UserResponseDto> GetByIdAsync(Guid id);
```

Reason: Ownership validation is no longer needed in the service
because the controller enforces it via:
- [Authorize] on /me → only own profile via CurrentUserId
- [Authorize(Roles = "Admin")] on /{id} → Admin only

## 5. Simplify UserService.GetByIdAsync
**Location:** `ExpenseManager.Application/Services/UserService.cs`

Change from:
```csharp
public async Task<UserResponseDto> GetByIdAsync(
    Guid id, Guid currentUserId, bool isAdmin)
{
    if (!isAdmin && id != currentUserId)
        throw new UnauthorizedException(...);

    var user = await _userRepository.GetByIdAsync(id);
    if (user == null)
        throw new NotFoundException(nameof(User), id);

    return _mapper.Map<UserResponseDto>(user);
}
```

To:
```csharp
public async Task<UserResponseDto> GetByIdAsync(Guid id)
{
    var user = await _userRepository.GetByIdAsync(id);
    if (user == null)
        throw new NotFoundException(nameof(User), id);

    return _mapper.Map<UserResponseDto>(user);
}
```

Reason: Authorization is handled at controller level via
[Authorize] and [Authorize(Roles = "Admin")] attributes.
Service only needs to fetch and return the user.

## Important Rules
- Never add UserId to request DTOs
- Never add IsPredefined to request DTOs
- Controller enforces who can access what via [Authorize] attributes
- Service layer handles business logic only
- UserId always comes from JWT claims via CurrentUserId