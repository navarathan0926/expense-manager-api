using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexesAndReceiptLineItemCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Receipts_UserId",
                table: "Receipts");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_UserId",
                table: "Expenses");

            migrationBuilder.AddColumn<int>(
                name: "LineItemCount",
                table: "Receipts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
                UPDATE "Receipts"
                SET "LineItemCount" = CASE
                    WHEN "LineItemsJson" IS NULL THEN 0
                    ELSE jsonb_array_length("LineItemsJson")
                END;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_UserId_CreatedAt",
                table: "Receipts",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_UserId_CategoryId",
                table: "Expenses",
                columns: new[] { "UserId", "CategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_UserId_Date",
                table: "Expenses",
                columns: new[] { "UserId", "Date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Receipts_UserId_CreatedAt",
                table: "Receipts");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_UserId_CategoryId",
                table: "Expenses");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_UserId_Date",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "LineItemCount",
                table: "Receipts");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_UserId",
                table: "Receipts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_UserId",
                table: "Expenses",
                column: "UserId");
        }
    }
}
