using ExpenseManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace ExpenseManager.Infrastructure.Persistence
{
	public class ExpenseManagerDbContext : DbContext
	{
		public ExpenseManagerDbContext(DbContextOptions<ExpenseManagerDbContext> options) : base(options) { }

		public DbSet<Category> Categories { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
		}
	}
}
