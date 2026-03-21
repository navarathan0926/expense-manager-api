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
			base.OnModelCreating(modelBuilder);
			modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
		}

		public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			var entries = ChangeTracker.Entries<BaseEntity>()
						.Where(e => e.State == EntityState.Added ||
						e.State == EntityState.Modified);

			foreach (var entry in entries)
			{
				entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;

				if (entry.State == EntityState.Added)
					entry.Entity.CreatedAt = DateTimeOffset.UtcNow;

				if (entry.Entity.IsDeleted && entry.Entity.DeletedAt == null)
        			entry.Entity.DeletedAt = DateTimeOffset.UtcNow;
			}

			return await base.SaveChangesAsync(cancellationToken);
		}
	}
}
