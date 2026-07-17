# Project: ExpenseManager API

**Stack:** .NET 8 Web API, Clean Architecture, PostgreSQL, EF Core

## 1. Architecture & Decisions

- **Clean Architecture:** The project follows a clean architecture pattern with four layers:
  - `ExpenseManager.API`: The presentation layer, responsible for handling HTTP requests, controllers, and middleware.
  - `ExpenseManager.Application`: The application layer, containing business logic, services, DTOs, validators, and AutoMapper profiles.
  - `ExpenseManager.Domain`: The domain layer, containing the core entities and enums. It has no external dependencies.
  - `ExpenseManager.Infrastructure`: The infrastructure layer, responsible for data access, database migrations, and other external concerns.
- **Database:** PostgreSQL is used as the database, with Entity Framework Core as the ORM.
- **Authentication & Authorization:** Not yet implemented. The plan is to use JWT with role-based authorization.
- **Validation:** FluentValidation is used for input validation. Validators are located in the `ExpenseManager.Application/Validators` directory.
- **Soft Deletes:** All entities inherit from a `BaseEntity` class that includes an `IsDeleted` flag. A global query filter is configured in `ExpenseManagerDbContext` to automatically exclude soft-deleted records from all queries. The `SaveChangesAsync` method is overridden to handle soft deletes automatically.
- **Multi-Currency:** The `Expense` entity includes `Currency` and `ExchangeRate` fields, but full multi-currency conversion is not yet implemented.
- **Dependency Injection:** Dependency injection is configured in the `AddApplication` and `AddInfrastructure` extension methods in the `ExpenseManager.Application` and `ExpenseManager.Infrastructure` projects, respectively.
- **Service Layer:** Services with external dependencies (like `CsvExportService` using CsvHelper, or `JwtTokenGenerator` using JWT libraries) are implemented in the `Infrastructure` layer, while their interfaces reside in the `Application` layer. Services with pure business logic remain in the `Application` layer.

## 2. Completed Work

- **Project Structure:** The solution is set up with the four clean architecture projects.
- **Entities:** The `User`, `Expense`, `Category`, and `BaseEntity` entities have been created in the `ExpenseManager.Domain` project.
- **Database Context:** `ExpenseManagerDbContext` is configured with `DbSet`s for the entities and an overridden `SaveChangesAsync` for soft deletes.
- **Migrations:** Initial database migrations have been created and applied.
- **DTOs:** Several DTOs have been created in the `ExpenseManager.Application/DTOs` directory.
- **Validators:** FluentValidation validators have been implemented for the DTOs.
- **Dependency Injection:** Basic dependency injection has been set up for the `Application` and `Infrastructure` layers.

## 3. Entities

- **`BaseEntity`**: An abstract base class with `Id`, `CreatedAt`, `UpdatedAt`, `IsDeleted`, and `DeletedAt` properties.
- **`User`**: Represents a user of the application. Includes `Email`, `PasswordHash`, `UserName`, and `Role` properties.
- **`Category`**: Represents an expense category. Can be predefined or user-created.
- **`Expense`**: Represents an expense. Linked to a `User` and a `Category`. Includes `Amount`, `Currency`, `Date`, and other details.

## 4. Configuration

- **`appsettings.json`**: Contains the connection string for the PostgreSQL database.
- **`launchSettings.json`**: Configures the application's launch profiles.

## 5. Decisions Made So Far

- **Clean Architecture:** Adopted for separation of concerns and maintainability.
- **PostgreSQL with EF Core:** Chosen for the database and ORM.
- **Soft Deletes:** Implemented to preserve data history.
- **FluentValidation:** Used for robust input validation.
- **`DateTimeOffset`:** Used for all date and time values to ensure timezone correctness.
- **Decimal Precision:** `Amount` properties are configured with `(18, 4)` precision, and `ExchangeRate` with `(10, 6)` for financial accuracy.
- **Delete Behavior:** Foreign key relationships use `DeleteBehavior.Restrict` to prevent accidental data loss.

## Phase 4 Development Plan

## Decisions

- Repository pattern WITHOUT Unit of Work
- Services inject repositories directly
- Exceptions thrown from services, caught by global middleware
- Password hashing with BCrypt.Net
- JWT authentication with role-based authorization (User/Admin)
- CsvHelper library for CSV export

## Role-Based Access Rules

- Both User and Admin can CRUD their own expenses and categories
- Predefined categories are readable by everyone
- Neither User nor Admin can access another person's expenses or categories
- Admin exclusively can manage users (list all users, get user by id)
- Ownership is always validated before update/delete/view of expenses and categories
- UserId is extracted from JWT claims, never trusted from request body for ownership

## 1. Custom Exceptions (Application/Exceptions/)

- NotFoundException(string name, object key)
- ConflictException(string message)
- UnauthorizedException(string message)

## 2. Repository Interfaces (Application/Repositories/)

IRepository<T>:

- GetByIdAsync, GetAllAsync, AddAsync, Update, Delete

IExpenseRepository extends IRepository<Expense>:

- GetByIdWithDetailsAsync(Guid id) — includes Category and User
- GetFilteredAsync(Guid userId, ExpenseFilterDto filters)

IUserRepository extends IRepository<User>:

- GetByEmailAsync(string email)
- GetAllUsersAsync() — Admin only use case

## 3. Repository Implementations (Infrastructure/Repositories/)

- Repository<T> : IRepository<T>
- ExpenseRepository : Repository<Expense>, IExpenseRepository
  - GetFilteredAsync must support: FromDate, ToDate,
    CategoryId, MinAmount, MaxAmount, Currency filters
- UserRepository : Repository<User>, IUserRepository

## 4. Service Interfaces (Application/Interfaces/)

IAuthService:

- RegisterAsync, LoginAsync

IExpenseService:

- GetAllAsync(Guid userId, ExpenseFilterDto filters)
- GetByIdAsync(Guid id, Guid userId)
- CreateAsync(CreateExpenseDto dto, Guid userId)
- UpdateAsync(Guid id, UpdateExpenseDto dto, Guid userId)
- DeleteAsync(Guid id, Guid userId)

ICategoryService:

- GetAllAsync(Guid userId) — predefined + own
- CreateAsync(CreateCategoryDto dto, Guid userId)

IReportService:

- GetMonthlySummaryAsync(Guid userId, int year, int month)
- GetCategoryBreakdownAsync(Guid userId, int year, int month)

ICsvExportService:

- ExportExpensesAsync(Guid userId, ExpenseFilterDto filters) → byte[]

IUserService (Admin only):

- GetAllUsersAsync() → IEnumerable<UserResponseDto>
- GetByIdAsync(Guid id) → UserResponseDto

## 5. Service Implementations (Application/Services/)

AuthService:

- Register: check duplicate email (ConflictException), BCrypt hash,
  save user, return UserResponseDto
- Login: find by email (NotFoundException), verify BCrypt
  (UnauthorizedException), generate JWT with claims (Id, Email, Role)
- JWT config from appsettings: JwtSettings:Secret, Issuer, Audience, ExpiryMinutes

ExpenseService:

- UserId always comes from JWT claims parameter, never from DTO
- Always validate ownership before update/delete/view (UnauthorizedException)
- Soft delete only (IsDeleted = true, never remove from DB)
- GetAll applies all ExpenseFilterDto filters scoped to userId

CategoryService:

- GetAll returns predefined categories + requesting user's own categories only
- Create checks duplicate name per user (ConflictException)
- UserId always comes from JWT claims parameter

ReportService:

- MonthlySummary: total amount, transaction count, average per month
- CategoryBreakdown: total amount grouped by category for given month
- Always scoped to requesting userId

CsvExportService:

- Use CsvHelper
- Columns: Date, Amount, Currency, Category, Description
- Always scoped to requesting userId

UserService (Admin only):

- GetAllUsers: return all non-deleted users
- GetById: return single user or throw NotFoundException

## 6. Global Exception Middleware (API/Middleware/)

- NotFoundException → 404
- ConflictException → 409
- UnauthorizedException → 401
- All others → 500
- Consistent JSON response: { "statusCode": int, "message": string }
- Log all exceptions using ILogger

## 7. Register in DependencyInjection.cs

Application/DependencyInjection.cs:

- AddAutoMapper, AddScoped for all services and interfaces

Infrastructure/DependencyInjection.cs:

- AddScoped for all repository implementations

## Important Rules

- async/await throughout, use CancellationToken where applicable
- Never store plain text passwords
- JWT secret from configuration only, never hardcoded
- decimal for all monetary values, never float or double
- UserId must always be extracted from JWT claims in controllers
  and passed down to services — never trust userId from request body
