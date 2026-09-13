using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Coupons.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "coupons");

            migrationBuilder.CreateTable(
                name: "Coupons",
                schema: "coupons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LaunchId = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "VARCHAR(50)", nullable: false),
                    Type = table.Column<string>(type: "VARCHAR(20)", nullable: false),
                    Value = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MaxDiscountAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    MaxRedemptions = table.Column<int>(type: "INT", nullable: false),
                    RedeemedCount = table.Column<int>(type: "INT", nullable: false),
                    ValidFrom = table.Column<DateTimeOffset>(type: "DATETIME", nullable: false),
                    ValidUntil = table.Column<DateTimeOffset>(type: "DATETIME", nullable: false),
                    MinimumOrderAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    MaxRedemptionsPerCustomer = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "VARCHAR(20)", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coupons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InboxMessages",
                schema: "coupons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "VARCHAR(200)", nullable: false),
                    Content = table.Column<string>(type: "JSONB", nullable: false),
                    OccurredOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ProcessedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Error = table.Column<string>(type: "VARCHAR(256)", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    NextRetryAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsPermanentFailure = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                schema: "coupons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "VARCHAR(200)", nullable: false),
                    Content = table.Column<string>(type: "JSONB", nullable: false),
                    OccurredOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ProcessedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Error = table.Column<string>(type: "VARCHAR(256)", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    NextRetryAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsPermanentFailure = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                schema: "coupons",
                columns: table => new
                {
                    RoleName = table.Column<string>(type: "VARCHAR(50)", nullable: false),
                    PermissionCode = table.Column<string>(type: "VARCHAR(100)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => new { x.RoleName, x.PermissionCode });
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                schema: "coupons",
                columns: table => new
                {
                    IdentityProviderId = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    RoleName = table.Column<string>(type: "VARCHAR(50)", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.IdentityProviderId, x.RoleName });
                });

            migrationBuilder.CreateTable(
                name: "CouponRedemption",
                schema: "coupons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CouponId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponRedemption", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CouponRedemption_Coupons_CouponId",
                        column: x => x.CouponId,
                        principalSchema: "coupons",
                        principalTable: "Coupons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InboxMessageConsumers",
                schema: "coupons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InboxMessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "VARCHAR(256)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboxMessageConsumers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InboxMessageConsumers_InboxMessages_InboxMessageId",
                        column: x => x.InboxMessageId,
                        principalSchema: "coupons",
                        principalTable: "InboxMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessageConsumers",
                schema: "coupons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OutboxMessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "VARCHAR(256)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessageConsumers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OutboxMessageConsumers_OutboxMessages_OutboxMessageId",
                        column: x => x.OutboxMessageId,
                        principalSchema: "coupons",
                        principalTable: "OutboxMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CouponRedemption_CouponId",
                schema: "coupons",
                table: "CouponRedemption",
                column: "CouponId");

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_Code",
                schema: "coupons",
                table: "Coupons",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessageConsumers_InboxMessageId_Name",
                schema: "coupons",
                table: "InboxMessageConsumers",
                columns: new[] { "InboxMessageId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessages_CorrelationId",
                schema: "coupons",
                table: "InboxMessages",
                column: "CorrelationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessageConsumers_OutboxMessageId_Name",
                schema: "coupons",
                table: "OutboxMessageConsumers",
                columns: new[] { "OutboxMessageId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_CorrelationId",
                schema: "coupons",
                table: "OutboxMessages",
                column: "CorrelationId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CouponRedemption",
                schema: "coupons");

            migrationBuilder.DropTable(
                name: "InboxMessageConsumers",
                schema: "coupons");

            migrationBuilder.DropTable(
                name: "OutboxMessageConsumers",
                schema: "coupons");

            migrationBuilder.DropTable(
                name: "RolePermissions",
                schema: "coupons");

            migrationBuilder.DropTable(
                name: "UserRoles",
                schema: "coupons");

            migrationBuilder.DropTable(
                name: "Coupons",
                schema: "coupons");

            migrationBuilder.DropTable(
                name: "InboxMessages",
                schema: "coupons");

            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "coupons");
        }
    }
}
