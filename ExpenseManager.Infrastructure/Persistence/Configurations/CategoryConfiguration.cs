using ExpenseManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseManager.Infrastructure.Persistence.Configurations
{
    public class CategoryConfiguration : BaseEntityConfiguration<Category>
    {
        public override void Configure(EntityTypeBuilder<Category> builder)
        {
            base.Configure(builder);

            builder.ToTable("Categories");

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Description)
                .HasMaxLength(250);

            builder.Property(c => c.IsPredefined)
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasOne(c => c.User)
                .WithMany(u => u.Categories)
                .HasForeignKey(c => c.UserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasData(
                new Category
                {
                    Id = Guid.Parse("a1b2c3d4-0001-0000-0000-000000000001"),
                    Name = "Food & Dining",
                    Description = "Restaurants, groceries, and food delivery",
                    IsPredefined = true,
                    UserId = null,
                    CreatedAt = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    UpdatedAt = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    IsDeleted = false,
                    DeletedAt = null
                },
                new Category
                {
                    Id = Guid.Parse("a1b2c3d4-0001-0000-0000-000000000002"),
                    Name = "Transport",
                    Description = "Fuel, public transit, ride-sharing, and vehicle maintenance",
                    IsPredefined = true,
                    UserId = null,
                    CreatedAt = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    UpdatedAt = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    IsDeleted = false,
                    DeletedAt = null
                },
                new Category
                {
                    Id = Guid.Parse("a1b2c3d4-0001-0000-0000-000000000003"),
                    Name = "Health & Medical",
                    Description = "Doctor visits, medications, and health insurance",
                    IsPredefined = true,
                    UserId = null,
                    CreatedAt = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    UpdatedAt = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    IsDeleted = false,
                    DeletedAt = null
                }
            );
        }
    }
}
