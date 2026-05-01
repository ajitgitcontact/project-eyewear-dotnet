using System;
using Microsoft.EntityFrameworkCore.Migrations;
using backend.Models.Orders;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscountCouponsAndOrderSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:address_type", "BILLING,SHIPPING")
                .Annotation("Npgsql:Enum:discount_applies_to", "all,product")
                .Annotation("Npgsql:Enum:discount_value_type", "flat,percentage")
                .Annotation("Npgsql:Enum:order_status", "CANCELLED,CONFIRMED,CREATED,DELIVERED,SHIPPED")
                .Annotation("Npgsql:Enum:payment_method", "CARD,COD,PREPAID,UPI")
                .Annotation("Npgsql:Enum:payment_status", "FAILED,PAID,PENDING")
                .Annotation("Npgsql:Enum:payment_txn_status", "FAILED,INITIATED,SUCCESS")
                .OldAnnotation("Npgsql:Enum:address_type", "BILLING,SHIPPING")
                .OldAnnotation("Npgsql:Enum:order_status", "CANCELLED,CONFIRMED,CREATED,DELIVERED,SHIPPED")
                .OldAnnotation("Npgsql:Enum:payment_method", "CARD,COD,PREPAID,UPI")
                .OldAnnotation("Npgsql:Enum:payment_status", "FAILED,PAID,PENDING")
                .OldAnnotation("Npgsql:Enum:payment_txn_status", "FAILED,INITIATED,SUCCESS");

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "OrderStatusLogs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EventType",
                table: "OrderStatusLogs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "ORDER_STATUS_UPDATED");

            migrationBuilder.AddColumn<string>(
                name: "LogMessage",
                table: "OrderStatusLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<PaymentStatus>(
                name: "PaymentStatus",
                table: "OrderStatusLogs",
                type: "payment_status",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CouponCode",
                table: "Orders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CouponDiscountAmount",
                table: "Orders",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FinalAmount",
                table: "Orders",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OriginalSubtotal",
                table: "Orders",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ProductDiscountTotal",
                table: "Orders",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FinalLineTotal",
                table: "OrderItems",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FinalUnitPrice",
                table: "OrderItems",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OriginalUnitPrice",
                table: "OrderItems",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ProductDiscountAmount",
                table: "OrderItems",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql("""
                UPDATE "Orders"
                SET "OriginalSubtotal" = "TotalAmount",
                    "FinalAmount" = "TotalAmount"
                WHERE "OriginalSubtotal" = 0 AND "FinalAmount" = 0;
                """);

            migrationBuilder.Sql("""
                UPDATE "OrderItems"
                SET "OriginalUnitPrice" = "Price",
                    "FinalUnitPrice" = "Price",
                    "FinalLineTotal" = "TotalPrice"
                WHERE "OriginalUnitPrice" = 0 AND "FinalUnitPrice" = 0;
                """);

            migrationBuilder.Sql("""
                UPDATE "OrderStatusLogs"
                SET "LogMessage" = "Comment",
                    "EventType" = 'ORDER_STATUS_UPDATED'
                WHERE "LogMessage" IS NULL OR "EventType" = '';
                """);

            migrationBuilder.CreateTable(
                name: "Coupons",
                columns: table => new
                {
                    CouponId = table.Column<string>(type: "varchar", nullable: false),
                    CouponCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CouponName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    CouponType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CouponValue = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    MinimumOrderAmount = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    MaximumCouponAmount = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    UsageLimit = table.Column<int>(type: "integer", nullable: true),
                    PerUserUsageLimit = table.Column<int>(type: "integer", nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coupons", x => x.CouponId);
                    table.CheckConstraint("CK_Coupons_CouponValue_NonNegative", "\"CouponValue\" >= 0");
                    table.CheckConstraint("CK_Coupons_MaximumCouponAmount_NonNegative", "\"MaximumCouponAmount\" IS NULL OR \"MaximumCouponAmount\" >= 0");
                    table.CheckConstraint("CK_Coupons_MinimumOrderAmount_NonNegative", "\"MinimumOrderAmount\" IS NULL OR \"MinimumOrderAmount\" >= 0");
                });

            migrationBuilder.CreateTable(
                name: "Discounts",
                columns: table => new
                {
                    DiscountId = table.Column<string>(type: "varchar", nullable: false),
                    DiscountName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DiscountType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DiscountValue = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    AppliesTo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Discounts", x => x.DiscountId);
                    table.CheckConstraint("CK_Discounts_DiscountValue_NonNegative", "\"DiscountValue\" >= 0");
                });

            migrationBuilder.CreateTable(
                name: "CouponUsages",
                columns: table => new
                {
                    CouponUsageId = table.Column<string>(type: "varchar", nullable: false),
                    CouponId = table.Column<string>(type: "varchar", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CustomerOrderId = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    CouponCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CouponAmount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponUsages", x => x.CouponUsageId);
                    table.ForeignKey(
                        name: "FK_CouponUsages_Coupons_CouponId",
                        column: x => x.CouponId,
                        principalTable: "Coupons",
                        principalColumn: "CouponId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CouponUsages_Orders_CustomerOrderId",
                        column: x => x.CustomerOrderId,
                        principalTable: "Orders",
                        principalColumn: "CustomerOrderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CouponUsages_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DiscountProducts",
                columns: table => new
                {
                    DiscountProductId = table.Column<string>(type: "varchar", nullable: false),
                    DiscountId = table.Column<string>(type: "varchar", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscountProducts", x => x.DiscountProductId);
                    table.ForeignKey(
                        name: "FK_DiscountProducts_Discounts_DiscountId",
                        column: x => x.DiscountId,
                        principalTable: "Discounts",
                        principalColumn: "DiscountId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiscountProducts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Orders_CouponDiscountAmount_NonNegative",
                table: "Orders",
                sql: "\"CouponDiscountAmount\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Orders_FinalAmount_NonNegative",
                table: "Orders",
                sql: "\"FinalAmount\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Orders_OriginalSubtotal_NonNegative",
                table: "Orders",
                sql: "\"OriginalSubtotal\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Orders_ProductDiscountTotal_NonNegative",
                table: "Orders",
                sql: "\"ProductDiscountTotal\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderItems_FinalLineTotal_NonNegative",
                table: "OrderItems",
                sql: "\"FinalLineTotal\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderItems_FinalUnitPrice_NonNegative",
                table: "OrderItems",
                sql: "\"FinalUnitPrice\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderItems_OriginalUnitPrice_NonNegative",
                table: "OrderItems",
                sql: "\"OriginalUnitPrice\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderItems_ProductDiscountAmount_NonNegative",
                table: "OrderItems",
                sql: "\"ProductDiscountAmount\" >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_CouponCode",
                table: "Coupons",
                column: "CouponCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_IsActive",
                table: "Coupons",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_CouponUsages_CouponId",
                table: "CouponUsages",
                column: "CouponId");

            migrationBuilder.CreateIndex(
                name: "IX_CouponUsages_CustomerOrderId",
                table: "CouponUsages",
                column: "CustomerOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_CouponUsages_UserId",
                table: "CouponUsages",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountProducts_DiscountId_ProductId",
                table: "DiscountProducts",
                columns: new[] { "DiscountId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DiscountProducts_ProductId",
                table: "DiscountProducts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Discounts_IsActive",
                table: "Discounts",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CouponUsages");

            migrationBuilder.DropTable(
                name: "DiscountProducts");

            migrationBuilder.DropTable(
                name: "Coupons");

            migrationBuilder.DropTable(
                name: "Discounts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Orders_CouponDiscountAmount_NonNegative",
                table: "Orders");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Orders_FinalAmount_NonNegative",
                table: "Orders");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Orders_OriginalSubtotal_NonNegative",
                table: "Orders");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Orders_ProductDiscountTotal_NonNegative",
                table: "Orders");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderItems_FinalLineTotal_NonNegative",
                table: "OrderItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderItems_FinalUnitPrice_NonNegative",
                table: "OrderItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderItems_OriginalUnitPrice_NonNegative",
                table: "OrderItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderItems_ProductDiscountAmount_NonNegative",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "OrderStatusLogs");

            migrationBuilder.DropColumn(
                name: "EventType",
                table: "OrderStatusLogs");

            migrationBuilder.DropColumn(
                name: "LogMessage",
                table: "OrderStatusLogs");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "OrderStatusLogs");

            migrationBuilder.DropColumn(
                name: "CouponCode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CouponDiscountAmount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FinalAmount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OriginalSubtotal",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ProductDiscountTotal",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FinalLineTotal",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "FinalUnitPrice",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "OriginalUnitPrice",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ProductDiscountAmount",
                table: "OrderItems");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:address_type", "BILLING,SHIPPING")
                .Annotation("Npgsql:Enum:order_status", "CANCELLED,CONFIRMED,CREATED,DELIVERED,SHIPPED")
                .Annotation("Npgsql:Enum:payment_method", "CARD,COD,PREPAID,UPI")
                .Annotation("Npgsql:Enum:payment_status", "FAILED,PAID,PENDING")
                .Annotation("Npgsql:Enum:payment_txn_status", "FAILED,INITIATED,SUCCESS")
                .OldAnnotation("Npgsql:Enum:address_type", "BILLING,SHIPPING")
                .OldAnnotation("Npgsql:Enum:discount_applies_to", "all,product")
                .OldAnnotation("Npgsql:Enum:discount_value_type", "flat,percentage")
                .OldAnnotation("Npgsql:Enum:order_status", "CANCELLED,CONFIRMED,CREATED,DELIVERED,SHIPPED")
                .OldAnnotation("Npgsql:Enum:payment_method", "CARD,COD,PREPAID,UPI")
                .OldAnnotation("Npgsql:Enum:payment_status", "FAILED,PAID,PENDING")
                .OldAnnotation("Npgsql:Enum:payment_txn_status", "FAILED,INITIATED,SUCCESS");
        }
    }
}
