using ExpenseManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace ExpenseManager.Infrastructure.Persistence
{
	public class ExpenseManagerDbContext : DbContext
	{
		public ExpenseManagerDbContext(DbContextOptions<ExpenseManagerDbContext> options) : base(options) { }

		public DbSet<User> Users { get; set; }
		public DbSet<Expense> Expenses { get; set; }
		public DbSet<Category> Categories { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.ApplyConfigurationsFromAssembly(typeof(ExpenseManagerDbContext).Assembly);
			base.OnModelCreating(modelBuilder);
		}

		public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			var entries = ChangeTracker.Entries<BaseEntity>();

			foreach (var entry in entries)
			{
				switch (entry.State)
				{
					case EntityState.Added:
						entry.Entity.CreatedAt = DateTimeOffset.UtcNow;
						entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
						break;
					case EntityState.Modified:
						entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
						break;
					case EntityState.Deleted:
						entry.State = EntityState.Modified;
						entry.Entity.IsDeleted = true;
						entry.Entity.DeletedAt = DateTimeOffset.UtcNow;
						break;
				}
			}

			return await base.SaveChangesAsync(cancellationToken);
		}
	}
}
