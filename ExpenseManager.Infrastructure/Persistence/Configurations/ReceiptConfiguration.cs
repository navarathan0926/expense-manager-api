using ExpenseManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseManager.Infrastructure.Persistence.Configurations
{
    public class ReceiptConfiguration : BaseEntityConfiguration<Receipt>
    {
        public override void Configure(EntityTypeBuilder<Receipt> builder)
        {
            base.Configure(builder);

            builder.ToTable("Receipts");

            builder.Property(r => r.FileName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(r => r.BlobKey)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(r => r.FileUrl)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(r => r.ContentType)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(r => r.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(r => r.Merchant)
                .HasMaxLength(255);

            builder.Property(r => r.Currency)
                .HasMaxLength(3);

            builder.Property(r => r.OcrErrorMessage)
                .HasMaxLength(500);

            builder.Property(r => r.LineItemsJson)
                .HasColumnType("jsonb");

            builder.Property(r => r.LineItemCount)
                .HasDefaultValue(0);

            builder.Property(r => r.ConfirmedImportMode)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.HasOne(r => r.User)
                .WithMany(u => u.Receipts)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.SuggestedCategory)
                .WithMany()
                .HasForeignKey(r => r.SuggestedCategoryId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(r => new { r.UserId, r.CreatedAt });
        }
    }
}
