using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReceiptOcrFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Receipts",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Merchant",
                table: "Receipts",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OcrErrorMessage",
                table: "Receipts",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "OcrProcessedAt",
                table: "Receipts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SuggestedCategoryId",
                table: "Receipts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmount",
                table: "Receipts",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "Receipts",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "TransactionDate",
                table: "Receipts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_SuggestedCategoryId",
                table: "Receipts",
                column: "SuggestedCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Receipts_Categories_SuggestedCategoryId",
                table: "Receipts",
                column: "SuggestedCategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Receipts_Categories_SuggestedCategoryId",
                table: "Receipts");

            migrationBuilder.DropIndex(
                name: "IX_Receipts_SuggestedCategoryId",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "Merchant",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "OcrErrorMessage",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "OcrProcessedAt",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "SuggestedCategoryId",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "TaxAmount",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "TransactionDate",
                table: "Receipts");
        }
    }
}
