using System;
using Microsoft.EntityFrameworkCore.Migrations;
using backend.Models.Orders;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderManagementSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:payment_status", "PENDING,PAID,FAILED")
                .Annotation("Npgsql:Enum:order_status", "CREATED,CONFIRMED,SHIPPED,DELIVERED,CANCELLED")
                .Annotation("Npgsql:Enum:payment_method", "COD,PREPAID,UPI,CARD")
                .Annotation("Npgsql:Enum:payment_txn_status", "INITIATED,SUCCESS,FAILED")
                .Annotation("Npgsql:Enum:address_type", "SHIPPING,BILLING");

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    orders_id = table.Column<string>(type: "varchar", nullable: false),
                    customer_order_id = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    customer_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    customer_email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    customer_phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    total_amount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    payment_status = table.Column<PaymentStatus>(type: "payment_status", nullable: false),
                    order_status = table.Column<OrderStatus>(type: "order_status", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orders", x => x.orders_id);
                    table.UniqueConstraint("AK_orders_customer_order_id", x => x.customer_order_id);
                    table.CheckConstraint("CK_orders_total_amount_non_negative", "total_amount >= 0");
                    table.ForeignKey(
                        name: "FK_orders_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "customer_prescriptions",
                columns: table => new
                {
                    customer_prescriptions_id = table.Column<string>(type: "varchar", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    customer_order_id = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    right_sphere = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    right_cylinder = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    right_axis = table.Column<int>(type: "integer", nullable: true),
                    right_add = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    left_sphere = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    left_cylinder = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    left_axis = table.Column<int>(type: "integer", nullable: true),
                    left_add = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    pd = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_prescriptions", x => x.customer_prescriptions_id);
                    table.CheckConstraint("CK_customer_prescriptions_left_axis_range", "left_axis IS NULL OR (left_axis >= 0 AND left_axis <= 180)");
                    table.CheckConstraint("CK_customer_prescriptions_pd_non_negative", "pd IS NULL OR pd >= 0");
                    table.CheckConstraint("CK_customer_prescriptions_right_axis_range", "right_axis IS NULL OR (right_axis >= 0 AND right_axis <= 180)");
                    table.ForeignKey(
                        name: "FK_customer_prescriptions_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_customer_prescriptions_orders_customer_order_id",
                        column: x => x.customer_order_id,
                        principalTable: "orders",
                        principalColumn: "customer_order_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_addresses",
                columns: table => new
                {
                    order_addresses_id = table.Column<string>(type: "varchar", nullable: false),
                    customer_order_id = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    type = table.Column<AddressType>(type: "address_type", nullable: false),
                    line1 = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    line2 = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    state = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    pincode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_addresses", x => x.order_addresses_id);
                    table.ForeignKey(
                        name: "FK_order_addresses_orders_customer_order_id",
                        column: x => x.customer_order_id,
                        principalTable: "orders",
                        principalColumn: "customer_order_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_items",
                columns: table => new
                {
                    order_items_id = table.Column<string>(type: "varchar", nullable: false),
                    customer_order_id = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    sku = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    product_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    total_price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_items", x => x.order_items_id);
                    table.CheckConstraint("CK_order_items_price_non_negative", "price >= 0");
                    table.CheckConstraint("CK_order_items_quantity_positive", "quantity > 0");
                    table.CheckConstraint("CK_order_items_total_price_non_negative", "total_price >= 0");
                    table.ForeignKey(
                        name: "FK_order_items_Products_product_id",
                        column: x => x.product_id,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_order_items_orders_customer_order_id",
                        column: x => x.customer_order_id,
                        principalTable: "orders",
                        principalColumn: "customer_order_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_status_logs",
                columns: table => new
                {
                    order_status_logs_id = table.Column<string>(type: "varchar", nullable: false),
                    customer_order_id = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    status = table.Column<OrderStatus>(type: "order_status", nullable: false),
                    comment = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_status_logs", x => x.order_status_logs_id);
                    table.ForeignKey(
                        name: "FK_order_status_logs_orders_customer_order_id",
                        column: x => x.customer_order_id,
                        principalTable: "orders",
                        principalColumn: "customer_order_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    payments_id = table.Column<string>(type: "varchar", nullable: false),
                    customer_order_id = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    method = table.Column<PaymentMethod>(type: "payment_method", nullable: false),
                    transaction_id = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    amount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    status = table.Column<PaymentTxnStatus>(type: "payment_txn_status", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.payments_id);
                    table.CheckConstraint("CK_payments_amount_non_negative", "amount >= 0");
                    table.ForeignKey(
                        name: "FK_payments_orders_customer_order_id",
                        column: x => x.customer_order_id,
                        principalTable: "orders",
                        principalColumn: "customer_order_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_item_customizations",
                columns: table => new
                {
                    order_item_customizations_id = table.Column<string>(type: "varchar", nullable: false),
                    order_item_id = table.Column<string>(type: "varchar", nullable: false),
                    customization_option_id = table.Column<int>(type: "integer", nullable: true),
                    customization_value_id = table.Column<int>(type: "integer", nullable: true),
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_item_customizations", x => x.order_item_customizations_id);
                    table.ForeignKey(
                        name: "FK_order_item_customizations_CustomizationOptions_customizatio~",
                        column: x => x.customization_option_id,
                        principalTable: "CustomizationOptions",
                        principalColumn: "CustomizationOptionId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_order_item_customizations_CustomizationValues_customization~",
                        column: x => x.customization_value_id,
                        principalTable: "CustomizationValues",
                        principalColumn: "CustomizationValueId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_order_item_customizations_order_items_order_item_id",
                        column: x => x.order_item_id,
                        principalTable: "order_items",
                        principalColumn: "order_items_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_customer_prescriptions_customer_order_id",
                table: "customer_prescriptions",
                column: "customer_order_id");

            migrationBuilder.CreateIndex(
                name: "IX_customer_prescriptions_user_id",
                table: "customer_prescriptions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_addresses_customer_order_id",
                table: "order_addresses",
                column: "customer_order_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_item_customizations_customization_option_id",
                table: "order_item_customizations",
                column: "customization_option_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_item_customizations_customization_value_id",
                table: "order_item_customizations",
                column: "customization_value_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_item_customizations_order_item_id",
                table: "order_item_customizations",
                column: "order_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_items_customer_order_id",
                table: "order_items",
                column: "customer_order_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_items_product_id",
                table: "order_items",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_items_sku",
                table: "order_items",
                column: "sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_order_status_logs_customer_order_id",
                table: "order_status_logs",
                column: "customer_order_id");

            migrationBuilder.CreateIndex(
                name: "IX_orders_customer_order_id",
                table: "orders",
                column: "customer_order_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_orders_user_id",
                table: "orders",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_payments_customer_order_id",
                table: "payments",
                column: "customer_order_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "customer_prescriptions");

            migrationBuilder.DropTable(
                name: "order_addresses");

            migrationBuilder.DropTable(
                name: "order_item_customizations");

            migrationBuilder.DropTable(
                name: "order_status_logs");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "order_items");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:Enum:payment_status", "PENDING,PAID,FAILED")
                .OldAnnotation("Npgsql:Enum:order_status", "CREATED,CONFIRMED,SHIPPED,DELIVERED,CANCELLED")
                .OldAnnotation("Npgsql:Enum:payment_method", "COD,PREPAID,UPI,CARD")
                .OldAnnotation("Npgsql:Enum:payment_txn_status", "INITIATED,SUCCESS,FAILED")
                .OldAnnotation("Npgsql:Enum:address_type", "SHIPPING,BILLING");
        }
    }
}
