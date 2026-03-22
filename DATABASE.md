## Schema Overview

### BaseEntity
All entities inherit from `BaseEntity` which provides:

| Column | Type | Description |
|--------|------|-------------|
| Id | uuid | Primary key |
| CreatedAt | timestamp with time zone | Record creation timestamp |
| UpdatedAt | timestamp with time zone | Last update timestamp |
| IsDeleted | boolean | Soft delete flag |
| DeletedAt | timestamp with time zone? | Deletion timestamp |

---

### Users Table

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | uuid | No | Primary key |
| Email | character varying(256) | No | Unique, indexed |
| PasswordHash | text | No | Bcrypt hashed |
| UserName | character varying(100) | No | Display name |
| Role | character varying(20) | No | User / Admin |
| CreatedAt | timestamp with time zone | No | |
| UpdatedAt | timestamp with time zone | No | |
| IsDeleted | boolean | No | Soft delete |
| DeletedAt | timestamp with time zone | Yes | |

---

### Categories Table

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | uuid | No | Primary key |
| Name | character varying(100) | No | Category name |
| Description | character varying(250) | Yes | Optional description |
| IsPredefined | boolean | No | System vs user category |
| UserId | uuid | Yes | Null for predefined categories |
| CreatedAt | timestamp with time zone | No | |
| UpdatedAt | timestamp with time zone | No | |
| IsDeleted | boolean | No | Soft delete |
| DeletedAt | timestamp with time zone | Yes | |

---

### Expenses Table

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | uuid | No | Primary key |
| UserId | uuid | No | FK → Users |
| Amount | numeric(18,4) | No | Expense amount |
| Currency | character varying(3) | No | ISO 4217 code e.g. USD |
| ExchangeRate | numeric(10,6) | Yes | Reserved for future use |
| CategoryId | uuid | No | FK → Categories |
| Description | character varying(500) | Yes | Optional note |
| Date | timestamp with time zone | No | Expense date |
| CreatedAt | timestamp with time zone | No | |
| UpdatedAt | timestamp with time zone | No | |
| IsDeleted | boolean | No | Soft delete |
| DeletedAt | timestamp with time zone | Yes | |

---

### __EFMigrationsHistory Table
| Column | Type | Description |
|--------|------|-------------|
| MigrationId | character varying(150) | Migration identifier |
| ProductVersion | character varying(32) | EF Core version |