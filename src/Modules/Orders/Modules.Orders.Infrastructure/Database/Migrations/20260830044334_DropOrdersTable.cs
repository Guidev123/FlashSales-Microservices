using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Orders.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class DropOrdersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Orders",
                schema: "orders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Orders",
                schema: "orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConfirmedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LaunchId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderCode = table.Column<string>(type: "VARCHAR(20)", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "VARCHAR(500)", nullable: true),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "VARCHAR(50)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerId_LaunchId",
                schema: "orders",
                table: "Orders",
                columns: new[] { "CustomerId", "LaunchId" },
                unique: true,
                filter: "\"Status\" IN ('AwaitingPayment', 'PaymentProcessing')");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerId_LaunchId_Status",
                schema: "orders",
                table: "Orders",
                columns: new[] { "CustomerId", "LaunchId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderCode",
                schema: "orders",
                table: "Orders",
                column: "OrderCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status_ExpiresAt",
                schema: "orders",
                table: "Orders",
                columns: new[] { "Status", "ExpiresAt" });
        }
    }
}
