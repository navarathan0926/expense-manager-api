## Multi-Currency Support

Each expense stores its original `Amount`, `Currency` (ISO 4217 code
e.g. USD, LKR), and an optional `ExchangeRate` field.

Full multi-currency conversion is not implemented in this version.
The `ExchangeRate` field is nullable and reserved for future implementation
where amounts can be normalized to a base currency for cross-currency
reporting and summaries.

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
