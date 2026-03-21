# AI Usage Report

**Project:** ExpenseManager API
**Stack:** .NET 8 Web API · Clean Architecture · PostgreSQL · EF Core

## 1. Prompts Used

### Project Initialization

> **Honest note:** The project was initialized with the help of ChatGPT, Claude, and official documentation. I used these tools as a sounding board to explore clean architecture patterns, compare structural options, and validate my thinking — not to generate the project blindly.

The specific areas I researched during initialization:

- Clean architecture layer responsibilities in the context of a .NET 8 Web API
- Whether and how Domain, Application, Infrastructure, and API layers should reference each other
- What project structure conventions look like for fintech-focused APIs

**Prompts used during initialization:**

> "I want to build a .NET 8 Web API for a fintech app using clean architecture. The app handles transactions and idempotency. Can you explain what each layer — Domain, Application, Infrastructure, API — should and should not contain, and what the project reference rules should be?"

> "What are the risks of letting the Domain layer depend on EF Core or any infrastructure concern? How do other teams handle shared base classes like audit fields?"

> "What does a clean architecture .NET solution folder structure typically look like? What goes in each project?"

**My decisions from this phase — owned entirely by me:**

| Decision                                             | Rationale                                                     |
| ---------------------------------------------------- | ------------------------------------------------------------- |
| Domain has zero external dependencies                | Keeps it fully unit-testable; no EF Core, no NuGet            |
| EF Core strictly in Infrastructure only              | Prevents domain model corruption by persistence concerns      |
| Idempotency at middleware level, not inside handlers | Cross-cutting concern; handlers should be pure business logic |
| `FluentValidation.AspNetCore` registered in API      | Pipeline registration is an HTTP-layer responsibility         |

These were validated through AI conversation and web research, but the final calls were mine.

---

### Phase 1 — Architecture & Project Setup

**Prompt 1**

> "I am building a .NET 8 Web API using clean architecture with layers: API, Application, Domain, Infrastructure. The project is a fintech app focusing on transactions and idempotency. What should each layer's responsibility be and what NuGet packages belong where?"

- **Why I asked this:** To validate my own understanding of clean architecture boundaries before writing code.
- **What I got:** A layer breakdown with package suggestions (MediatR in Application, EF Core in Infrastructure, FluentValidation in Application).
- **What I did:** Confirmed it matched my design. Moved `FluentValidation.AspNetCore` to the API layer instead — the AI placed it in Application, but registration belongs closer to the HTTP pipeline.

---

**Prompt**

> _(Suggestion from Copilot during entity review)_  
> Copilot flagged: "You have `Id`, `CreatedAt`, `UpdatedAt` scattered across entities.
> Consider extracting a base class."

- **What I got:** A suggestion to create a `BaseEntity` or `AuditableEntity` abstract
  class containing common fields repeated across domain entities.
- **My reasoning:** I agreed — this is a straightforward application of the **DRY
  principle** (Don't Repeat Yourself). Having `Id`, `CreatedAt`, and `UpdatedAt`
  duplicated across every entity is a maintenance risk: if the audit field type or
  naming changes, we might need to update every entity individually.
- **Decision:** ✅ Accepted and implemented as `BaseEntity` in the Domain layer
  with no external dependencies.

```


| Area                        | AI Tool Used                        |
|-----------------------------|-------------------------------------|
| Project initialization      | ChatGPT, Claude, documentation      |
| Entity code suggestions     | GitHub Copilot (inline)             |
| Architecture Q&A            | ChatGPT, Claude                     |
| Idempotency implementation  | Claude                              |
| EF Core configuration       | Claude                              |
| Unit & integration tests    | Need to implement    |


```

**Copilot Suggestion — DateTime vs DateTimeOffset**

> _(Inline suggestion during BaseEntity implementation)_  
> Copilot auto-completed audit fields using `DateTime` for `CreatedAt` and `UpdatedAt`.

- **What Copilot suggested:**

```csharp
public DateTime CreatedAt { get; internal set; }
public DateTime UpdatedAt { get; internal set; }
```

- **Decision:** Rejected — replaced with `DateTimeOffset`.

```csharp
public DateTimeOffset CreatedAt { get; internal set; }
public DateTimeOffset UpdatedAt { get; internal set; }
```

- **Why I rejected it:** `DateTime` does not carry timezone information.
  In a fintech application where transactions may originate from multiple
  timezones, storing a `DateTime` without offset context creates ambiguity —
  you cannot reliably determine whether a stored value is UTC, local, or
  unspecified. `DateTimeOffset` stores the UTC offset alongside the value,
  making the timezone context explicit and unambiguous at the data level.

- **Additional reasons:**
  - Npgsql maps `DateTimeOffset` to `timestamptz` (timestamp with time zone)
    in PostgreSQL — the correct column type for audit timestamps.
  - `DateTime` maps to `timestamp` (no timezone) — which PostgreSQL stores
    as-is with no timezone awareness, a known source of bugs in distributed systems.
  - For financial audit trails, regulators and auditors expect unambiguous
    timestamps. `DateTimeOffset` satisfies this requirement; `DateTime` does not.

### Entity Validation

**Prompt Given to Copilot:**
"Validate these domain entities for clean architecture in ASP.NET Core 8.
Check for correctness and fix any issues."

**What Copilot Changed:**

- Added `= null!` to `public User? User { get; set; }` in `Category.cs`
- Removed the default value `= false` from `IsPredefined` in `Category.cs`

**Why I Rejected These Changes:**

1. `User?` is a nullable navigation property — adding `= null!` contradicts
   the nullable declaration. `null!` is only appropriate for non-nullable
   properties where EF Core guarantees the value will be populated.
2. `IsPredefined = false` default was intentionally set to make the code
   explicit and self-documenting. When a user creates a category it should
   clearly default to non-predefined. Removing the default value loses
   that intent even though EF Core defaults bool to false anyway.

## DTO Generation

**Tool:** GitHub Copilot (GPT-4.1 Codex Max)

**Prompt:** "Using these entities generate the appropriate DTOs:
CreateCategoryDto, CreateExpenseDto, RegisterRequestDto,
UpdateExpenseDto, UserResponseDto, ExpenseResponseDto."

**Issue:** Copilot marked required fields as nullable (e.g., `string? Name`).

**Decision:** Rejected.  
Since `Name` is required in the entity, it should be non-nullable in DTOs.

**Fix:** Used non-nullable properties with `= null!` to avoid warnings and reflect correct data contract.

## Interaction 3 — Expense Entity Configuration & Precision Handling

**Tool:** GitHub Copilot (GPT-4.1 Codex Max)

**Prompt:** "Generate EF Core configuration for the Expense entity
with proper fintech decimal handling."

**What Copilot Generated:**

```csharp
builder.Property(e => e.Amount).HasPrecision(18, 2);      // ❌
builder.Property(e => e.ExchangeRate).HasPrecision(18, 6); // ❌
```

**Why I Rejected:**

- `(18, 2)` on Amount loses precision in financial calculations —
  rounding at 2 decimal places can accumulate errors across transactions
- `(18, 6)` on ExchangeRate is unnecessarily large for standard fiat
  currency pairs which rarely exceed 4 digits before the decimal

**Corrections Applied:**

```csharp
builder.Property(e => e.Amount).HasPrecision(18, 4);      // ✅
builder.Property(e => e.ExchangeRate).HasPrecision(10, 6); // ✅
```

## Interaction 4 — Category Delete Behavior

**Tool:** GitHub Copilot (GPT-4.1 Codex Max)

**What Copilot Generated:**

```csharp
builder.HasOne(c => c.User)
    .WithMany(u => u.Categories)
    .HasForeignKey(c => c.UserId)
    .IsRequired(false)
    .OnDelete(DeleteBehavior.SetNull); // ❌
```

**Why I Rejected This:**
`SetNull` would set `UserId` to null when a user is deleted, making
user-defined categories indistinguishable from predefined categories.
Since the application uses soft delete throughout, hard deletion never
occurs — making `SetNull` unnecessary and harmful to data integrity.

**Correction Applied:**

```csharp
.OnDelete(DeleteBehavior.Restrict); // ✅
```

**Decision:** Changed to `Restrict` across all relationships, relying
entirely on soft delete for record management.
