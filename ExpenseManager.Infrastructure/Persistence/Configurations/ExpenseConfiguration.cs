using ExpenseManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseManager.Infrastructure.Persistence.Configurations
{
    public class ExpenseConfiguration : BaseEntityConfiguration<Expense>
    {
        public override void Configure(EntityTypeBuilder<Expense> builder)
        {
            base.Configure(builder);

            builder.ToTable("Expenses");

            builder.Property(e => e.Amount)
                .HasPrecision(18, 4);

            builder.Property(e => e.Currency)
                .IsRequired()
                .HasMaxLength(3);

            builder.Property(e => e.ExchangeRate)
                .HasPrecision(10, 6);

            builder.Property(e => e.Description)
                .HasMaxLength(500);

            builder.HasOne(e => e.User)
                .WithMany(u => u.Expenses)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Category)
                .WithMany(c => c.Expenses)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
