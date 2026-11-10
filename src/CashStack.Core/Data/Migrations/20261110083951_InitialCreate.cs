using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CashStack.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, collation: "NOCASE"),
                    Type = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Timestamp = table.Column<long>(type: "INTEGER", nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    CategoryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Amount = table.Column<long>(type: "INTEGER", nullable: false),
                    Day = table.Column<DateOnly>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name", "Type" },
                values: new object[,]
                {
                    { new Guid("6f0b8a52-1c3e-4c1a-9a51-000000000001"), "Other", "Income" },
                    { new Guid("6f0b8a52-1c3e-4c1a-9a51-000000000002"), "Other", "Expense" },
                    { new Guid("6f0b8a52-1c3e-4c1a-9a51-000000000101"), "Salary", "Income" },
                    { new Guid("6f0b8a52-1c3e-4c1a-9a51-000000000102"), "Freelance", "Income" },
                    { new Guid("6f0b8a52-1c3e-4c1a-9a51-000000000201"), "Groceries", "Expense" },
                    { new Guid("6f0b8a52-1c3e-4c1a-9a51-000000000202"), "Rent", "Expense" },
                    { new Guid("6f0b8a52-1c3e-4c1a-9a51-000000000203"), "Transport", "Expense" },
                    { new Guid("6f0b8a52-1c3e-4c1a-9a51-000000000204"), "Leisure", "Expense" },
                    { new Guid("6f0b8a52-1c3e-4c1a-9a51-000000000205"), "Health", "Expense" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Type_Name",
                table: "Categories",
                columns: new[] { "Type", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CategoryId",
                table: "Transactions",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Day",
                table: "Transactions",
                column: "Day");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
