using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class RenameOrderSchemaToPascalCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_customer_prescriptions_Users_user_id",
                table: "customer_prescriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_customer_prescriptions_orders_customer_order_id",
                table: "customer_prescriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_order_addresses_orders_customer_order_id",
                table: "order_addresses");

            migrationBuilder.DropForeignKey(
                name: "FK_order_item_customizations_CustomizationOptions_customizatio~",
                table: "order_item_customizations");

            migrationBuilder.DropForeignKey(
                name: "FK_order_item_customizations_CustomizationValues_customization~",
                table: "order_item_customizations");

            migrationBuilder.DropForeignKey(
                name: "FK_order_item_customizations_order_items_order_item_id",
                table: "order_item_customizations");

            migrationBuilder.DropForeignKey(
                name: "FK_order_items_Products_product_id",
                table: "order_items");

            migrationBuilder.DropForeignKey(
                name: "FK_order_items_orders_customer_order_id",
                table: "order_items");

            migrationBuilder.DropForeignKey(
                name: "FK_order_status_logs_orders_customer_order_id",
                table: "order_status_logs");

            migrationBuilder.DropForeignKey(
                name: "FK_orders_Users_user_id",
                table: "orders");

            migrationBuilder.DropForeignKey(
                name: "FK_payments_orders_customer_order_id",
                table: "payments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_payments",
                table: "payments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_payments_amount_non_negative",
                table: "payments");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_orders_customer_order_id",
                table: "orders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_orders",
                table: "orders");

            migrationBuilder.DropCheckConstraint(
                name: "CK_orders_total_amount_non_negative",
                table: "orders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_order_status_logs",
                table: "order_status_logs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_order_items",
                table: "order_items");

            migrationBuilder.DropCheckConstraint(
                name: "CK_order_items_price_non_negative",
                table: "order_items");

            migrationBuilder.DropCheckConstraint(
                name: "CK_order_items_quantity_positive",
                table: "order_items");

            migrationBuilder.DropCheckConstraint(
                name: "CK_order_items_total_price_non_negative",
                table: "order_items");

            migrationBuilder.DropPrimaryKey(
                name: "PK_order_item_customizations",
                table: "order_item_customizations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_order_addresses",
                table: "order_addresses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_customer_prescriptions",
                table: "customer_prescriptions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_customer_prescriptions_left_axis_range",
                table: "customer_prescriptions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_customer_prescriptions_pd_non_negative",
                table: "customer_prescriptions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_customer_prescriptions_right_axis_range",
                table: "customer_prescriptions");

            migrationBuilder.RenameTable(
                name: "payments",
                newName: "Payments");

            migrationBuilder.RenameTable(
                name: "orders",
                newName: "Orders");

            migrationBuilder.RenameTable(
                name: "order_status_logs",
                newName: "OrderStatusLogs");

            migrationBuilder.RenameTable(
                name: "order_items",
                newName: "OrderItems");

            migrationBuilder.RenameTable(
                name: "order_item_customizations",
                newName: "OrderItemCustomizations");

            migrationBuilder.RenameTable(
                name: "order_addresses",
                newName: "OrderAddresses");

            migrationBuilder.RenameTable(
                name: "customer_prescriptions",
                newName: "CustomerPrescriptions");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "Payments",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "method",
                table: "Payments",
                newName: "Method");

            migrationBuilder.RenameColumn(
                name: "amount",
                table: "Payments",
                newName: "Amount");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "Payments",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "transaction_id",
                table: "Payments",
                newName: "TransactionId");

            migrationBuilder.RenameColumn(
                name: "customer_order_id",
                table: "Payments",
                newName: "CustomerOrderId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Payments",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "payments_id",
                table: "Payments",
                newName: "PaymentsId");

            migrationBuilder.RenameIndex(
                name: "IX_payments_customer_order_id",
                table: "Payments",
                newName: "IX_Payments_CustomerOrderId");

            migrationBuilder.RenameColumn(
                name: "notes",
                table: "Orders",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "Orders",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "Orders",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "total_amount",
                table: "Orders",
                newName: "TotalAmount");

            migrationBuilder.RenameColumn(
                name: "payment_status",
                table: "Orders",
                newName: "PaymentStatus");

            migrationBuilder.RenameColumn(
                name: "order_status",
                table: "Orders",
                newName: "OrderStatus");

            migrationBuilder.RenameColumn(
                name: "customer_phone",
                table: "Orders",
                newName: "CustomerPhone");

            migrationBuilder.RenameColumn(
                name: "customer_order_id",
                table: "Orders",
                newName: "CustomerOrderId");

            migrationBuilder.RenameColumn(
                name: "customer_name",
                table: "Orders",
                newName: "CustomerName");

            migrationBuilder.RenameColumn(
                name: "customer_email",
                table: "Orders",
                newName: "CustomerEmail");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Orders",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "orders_id",
                table: "Orders",
                newName: "OrdersId");

            migrationBuilder.RenameIndex(
                name: "IX_orders_user_id",
                table: "Orders",
                newName: "IX_Orders_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_orders_customer_order_id",
                table: "Orders",
                newName: "IX_Orders_CustomerOrderId");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "OrderStatusLogs",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "comment",
                table: "OrderStatusLogs",
                newName: "Comment");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "OrderStatusLogs",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "customer_order_id",
                table: "OrderStatusLogs",
                newName: "CustomerOrderId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "OrderStatusLogs",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "order_status_logs_id",
                table: "OrderStatusLogs",
                newName: "OrderStatusLogsId");

            migrationBuilder.RenameIndex(
                name: "IX_order_status_logs_customer_order_id",
                table: "OrderStatusLogs",
                newName: "IX_OrderStatusLogs_CustomerOrderId");

            migrationBuilder.RenameColumn(
                name: "sku",
                table: "OrderItems",
                newName: "SKU");

            migrationBuilder.RenameColumn(
                name: "quantity",
                table: "OrderItems",
                newName: "Quantity");

            migrationBuilder.RenameColumn(
                name: "price",
                table: "OrderItems",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "OrderItems",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "total_price",
                table: "OrderItems",
                newName: "TotalPrice");

            migrationBuilder.RenameColumn(
                name: "product_name",
                table: "OrderItems",
                newName: "ProductName");

            migrationBuilder.RenameColumn(
                name: "product_id",
                table: "OrderItems",
                newName: "ProductId");

            migrationBuilder.RenameColumn(
                name: "customer_order_id",
                table: "OrderItems",
                newName: "CustomerOrderId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "OrderItems",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "order_items_id",
                table: "OrderItems",
                newName: "OrderItemsId");

            migrationBuilder.RenameIndex(
                name: "IX_order_items_sku",
                table: "OrderItems",
                newName: "IX_OrderItems_SKU");

            migrationBuilder.RenameIndex(
                name: "IX_order_items_product_id",
                table: "OrderItems",
                newName: "IX_OrderItems_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_order_items_customer_order_id",
                table: "OrderItems",
                newName: "IX_OrderItems_CustomerOrderId");

            migrationBuilder.RenameColumn(
                name: "value",
                table: "OrderItemCustomizations",
                newName: "Value");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "OrderItemCustomizations",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "OrderItemCustomizations",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "order_item_id",
                table: "OrderItemCustomizations",
                newName: "OrderItemId");

            migrationBuilder.RenameColumn(
                name: "customization_value_id",
                table: "OrderItemCustomizations",
                newName: "CustomizationValueId");

            migrationBuilder.RenameColumn(
                name: "customization_option_id",
                table: "OrderItemCustomizations",
                newName: "CustomizationOptionId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "OrderItemCustomizations",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "order_item_customizations_id",
                table: "OrderItemCustomizations",
                newName: "OrderItemCustomizationsId");

            migrationBuilder.RenameIndex(
                name: "IX_order_item_customizations_order_item_id",
                table: "OrderItemCustomizations",
                newName: "IX_OrderItemCustomizations_OrderItemId");

            migrationBuilder.RenameIndex(
                name: "IX_order_item_customizations_customization_value_id",
                table: "OrderItemCustomizations",
                newName: "IX_OrderItemCustomizations_CustomizationValueId");

            migrationBuilder.RenameIndex(
                name: "IX_order_item_customizations_customization_option_id",
                table: "OrderItemCustomizations",
                newName: "IX_OrderItemCustomizations_CustomizationOptionId");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "OrderAddresses",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "state",
                table: "OrderAddresses",
                newName: "State");

            migrationBuilder.RenameColumn(
                name: "pincode",
                table: "OrderAddresses",
                newName: "Pincode");

            migrationBuilder.RenameColumn(
                name: "line2",
                table: "OrderAddresses",
                newName: "Line2");

            migrationBuilder.RenameColumn(
                name: "line1",
                table: "OrderAddresses",
                newName: "Line1");

            migrationBuilder.RenameColumn(
                name: "country",
                table: "OrderAddresses",
                newName: "Country");

            migrationBuilder.RenameColumn(
                name: "city",
                table: "OrderAddresses",
                newName: "City");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "OrderAddresses",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "customer_order_id",
                table: "OrderAddresses",
                newName: "CustomerOrderId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "OrderAddresses",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "order_addresses_id",
                table: "OrderAddresses",
                newName: "OrderAddressesId");

            migrationBuilder.RenameIndex(
                name: "IX_order_addresses_customer_order_id",
                table: "OrderAddresses",
                newName: "IX_OrderAddresses_CustomerOrderId");

            migrationBuilder.RenameColumn(
                name: "pd",
                table: "CustomerPrescriptions",
                newName: "PD");

            migrationBuilder.RenameColumn(
                name: "notes",
                table: "CustomerPrescriptions",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "CustomerPrescriptions",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "CustomerPrescriptions",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "right_sphere",
                table: "CustomerPrescriptions",
                newName: "RightSphere");

            migrationBuilder.RenameColumn(
                name: "right_cylinder",
                table: "CustomerPrescriptions",
                newName: "RightCylinder");

            migrationBuilder.RenameColumn(
                name: "right_axis",
                table: "CustomerPrescriptions",
                newName: "RightAxis");

            migrationBuilder.RenameColumn(
                name: "right_add",
                table: "CustomerPrescriptions",
                newName: "RightAdd");

            migrationBuilder.RenameColumn(
                name: "left_sphere",
                table: "CustomerPrescriptions",
                newName: "LeftSphere");

            migrationBuilder.RenameColumn(
                name: "left_cylinder",
                table: "CustomerPrescriptions",
                newName: "LeftCylinder");

            migrationBuilder.RenameColumn(
                name: "left_axis",
                table: "CustomerPrescriptions",
                newName: "LeftAxis");

            migrationBuilder.RenameColumn(
                name: "left_add",
                table: "CustomerPrescriptions",
                newName: "LeftAdd");

            migrationBuilder.RenameColumn(
                name: "customer_order_id",
                table: "CustomerPrescriptions",
                newName: "CustomerOrderId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "CustomerPrescriptions",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "customer_prescriptions_id",
                table: "CustomerPrescriptions",
                newName: "CustomerPrescriptionsId");

            migrationBuilder.RenameIndex(
                name: "IX_customer_prescriptions_user_id",
                table: "CustomerPrescriptions",
                newName: "IX_CustomerPrescriptions_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_customer_prescriptions_customer_order_id",
                table: "CustomerPrescriptions",
                newName: "IX_CustomerPrescriptions_CustomerOrderId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Payments",
                table: "Payments",
                column: "PaymentsId");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Orders_CustomerOrderId",
                table: "Orders",
                column: "CustomerOrderId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Orders",
                table: "Orders",
                column: "OrdersId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderStatusLogs",
                table: "OrderStatusLogs",
                column: "OrderStatusLogsId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderItems",
                table: "OrderItems",
                column: "OrderItemsId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderItemCustomizations",
                table: "OrderItemCustomizations",
                column: "OrderItemCustomizationsId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderAddresses",
                table: "OrderAddresses",
                column: "OrderAddressesId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CustomerPrescriptions",
                table: "CustomerPrescriptions",
                column: "CustomerPrescriptionsId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Payments_Amount_NonNegative",
                table: "Payments",
                sql: "\"Amount\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Orders_TotalAmount_NonNegative",
                table: "Orders",
                sql: "\"TotalAmount\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderItems_Price_NonNegative",
                table: "OrderItems",
                sql: "\"Price\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderItems_Quantity_Positive",
                table: "OrderItems",
                sql: "\"Quantity\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderItems_TotalPrice_NonNegative",
                table: "OrderItems",
                sql: "\"TotalPrice\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CustomerPrescriptions_LeftAxis_Range",
                table: "CustomerPrescriptions",
                sql: "\"LeftAxis\" IS NULL OR (\"LeftAxis\" >= 0 AND \"LeftAxis\" <= 180)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CustomerPrescriptions_PD_NonNegative",
                table: "CustomerPrescriptions",
                sql: "\"PD\" IS NULL OR \"PD\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CustomerPrescriptions_RightAxis_Range",
                table: "CustomerPrescriptions",
                sql: "\"RightAxis\" IS NULL OR (\"RightAxis\" >= 0 AND \"RightAxis\" <= 180)");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerPrescriptions_Orders_CustomerOrderId",
                table: "CustomerPrescriptions",
                column: "CustomerOrderId",
                principalTable: "Orders",
                principalColumn: "CustomerOrderId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerPrescriptions_Users_UserId",
                table: "CustomerPrescriptions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderAddresses_Orders_CustomerOrderId",
                table: "OrderAddresses",
                column: "CustomerOrderId",
                principalTable: "Orders",
                principalColumn: "CustomerOrderId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItemCustomizations_CustomizationOptions_CustomizationO~",
                table: "OrderItemCustomizations",
                column: "CustomizationOptionId",
                principalTable: "CustomizationOptions",
                principalColumn: "CustomizationOptionId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItemCustomizations_CustomizationValues_CustomizationVa~",
                table: "OrderItemCustomizations",
                column: "CustomizationValueId",
                principalTable: "CustomizationValues",
                principalColumn: "CustomizationValueId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItemCustomizations_OrderItems_OrderItemId",
                table: "OrderItemCustomizations",
                column: "OrderItemId",
                principalTable: "OrderItems",
                principalColumn: "OrderItemsId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Orders_CustomerOrderId",
                table: "OrderItems",
                column: "CustomerOrderId",
                principalTable: "Orders",
                principalColumn: "CustomerOrderId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Products_ProductId",
                table: "OrderItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Users_UserId",
                table: "Orders",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderStatusLogs_Orders_CustomerOrderId",
                table: "OrderStatusLogs",
                column: "CustomerOrderId",
                principalTable: "Orders",
                principalColumn: "CustomerOrderId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Orders_CustomerOrderId",
                table: "Payments",
                column: "CustomerOrderId",
                principalTable: "Orders",
                principalColumn: "CustomerOrderId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerPrescriptions_Orders_CustomerOrderId",
                table: "CustomerPrescriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerPrescriptions_Users_UserId",
                table: "CustomerPrescriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderAddresses_Orders_CustomerOrderId",
                table: "OrderAddresses");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItemCustomizations_CustomizationOptions_CustomizationO~",
                table: "OrderItemCustomizations");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItemCustomizations_CustomizationValues_CustomizationVa~",
                table: "OrderItemCustomizations");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItemCustomizations_OrderItems_OrderItemId",
                table: "OrderItemCustomizations");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Orders_CustomerOrderId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Products_ProductId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Users_UserId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderStatusLogs_Orders_CustomerOrderId",
                table: "OrderStatusLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Orders_CustomerOrderId",
                table: "Payments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Payments",
                table: "Payments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Payments_Amount_NonNegative",
                table: "Payments");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Orders_CustomerOrderId",
                table: "Orders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Orders",
                table: "Orders");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Orders_TotalAmount_NonNegative",
                table: "Orders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderStatusLogs",
                table: "OrderStatusLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderItems",
                table: "OrderItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderItems_Price_NonNegative",
                table: "OrderItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderItems_Quantity_Positive",
                table: "OrderItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderItems_TotalPrice_NonNegative",
                table: "OrderItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderItemCustomizations",
                table: "OrderItemCustomizations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderAddresses",
                table: "OrderAddresses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CustomerPrescriptions",
                table: "CustomerPrescriptions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CustomerPrescriptions_LeftAxis_Range",
                table: "CustomerPrescriptions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CustomerPrescriptions_PD_NonNegative",
                table: "CustomerPrescriptions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CustomerPrescriptions_RightAxis_Range",
                table: "CustomerPrescriptions");

            migrationBuilder.RenameTable(
                name: "Payments",
                newName: "payments");

            migrationBuilder.RenameTable(
                name: "Orders",
                newName: "orders");

            migrationBuilder.RenameTable(
                name: "OrderStatusLogs",
                newName: "order_status_logs");

            migrationBuilder.RenameTable(
                name: "OrderItems",
                newName: "order_items");

            migrationBuilder.RenameTable(
                name: "OrderItemCustomizations",
                newName: "order_item_customizations");

            migrationBuilder.RenameTable(
                name: "OrderAddresses",
                newName: "order_addresses");

            migrationBuilder.RenameTable(
                name: "CustomerPrescriptions",
                newName: "customer_prescriptions");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "payments",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Method",
                table: "payments",
                newName: "method");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "payments",
                newName: "amount");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "payments",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "TransactionId",
                table: "payments",
                newName: "transaction_id");

            migrationBuilder.RenameColumn(
                name: "CustomerOrderId",
                table: "payments",
                newName: "customer_order_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "payments",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "PaymentsId",
                table: "payments",
                newName: "payments_id");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_CustomerOrderId",
                table: "payments",
                newName: "IX_payments_customer_order_id");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "orders",
                newName: "notes");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "orders",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "orders",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "TotalAmount",
                table: "orders",
                newName: "total_amount");

            migrationBuilder.RenameColumn(
                name: "PaymentStatus",
                table: "orders",
                newName: "payment_status");

            migrationBuilder.RenameColumn(
                name: "OrderStatus",
                table: "orders",
                newName: "order_status");

            migrationBuilder.RenameColumn(
                name: "CustomerPhone",
                table: "orders",
                newName: "customer_phone");

            migrationBuilder.RenameColumn(
                name: "CustomerOrderId",
                table: "orders",
                newName: "customer_order_id");

            migrationBuilder.RenameColumn(
                name: "CustomerName",
                table: "orders",
                newName: "customer_name");

            migrationBuilder.RenameColumn(
                name: "CustomerEmail",
                table: "orders",
                newName: "customer_email");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "orders",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "OrdersId",
                table: "orders",
                newName: "orders_id");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_UserId",
                table: "orders",
                newName: "IX_orders_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_CustomerOrderId",
                table: "orders",
                newName: "IX_orders_customer_order_id");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "order_status_logs",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Comment",
                table: "order_status_logs",
                newName: "comment");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "order_status_logs",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CustomerOrderId",
                table: "order_status_logs",
                newName: "customer_order_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "order_status_logs",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "OrderStatusLogsId",
                table: "order_status_logs",
                newName: "order_status_logs_id");

            migrationBuilder.RenameIndex(
                name: "IX_OrderStatusLogs_CustomerOrderId",
                table: "order_status_logs",
                newName: "IX_order_status_logs_customer_order_id");

            migrationBuilder.RenameColumn(
                name: "SKU",
                table: "order_items",
                newName: "sku");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "order_items",
                newName: "quantity");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "order_items",
                newName: "price");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "order_items",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "TotalPrice",
                table: "order_items",
                newName: "total_price");

            migrationBuilder.RenameColumn(
                name: "ProductName",
                table: "order_items",
                newName: "product_name");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "order_items",
                newName: "product_id");

            migrationBuilder.RenameColumn(
                name: "CustomerOrderId",
                table: "order_items",
                newName: "customer_order_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "order_items",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "OrderItemsId",
                table: "order_items",
                newName: "order_items_id");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItems_SKU",
                table: "order_items",
                newName: "IX_order_items_sku");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItems_ProductId",
                table: "order_items",
                newName: "IX_order_items_product_id");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItems_CustomerOrderId",
                table: "order_items",
                newName: "IX_order_items_customer_order_id");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "order_item_customizations",
                newName: "value");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "order_item_customizations",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "order_item_customizations",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "OrderItemId",
                table: "order_item_customizations",
                newName: "order_item_id");

            migrationBuilder.RenameColumn(
                name: "CustomizationValueId",
                table: "order_item_customizations",
                newName: "customization_value_id");

            migrationBuilder.RenameColumn(
                name: "CustomizationOptionId",
                table: "order_item_customizations",
                newName: "customization_option_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "order_item_customizations",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "OrderItemCustomizationsId",
                table: "order_item_customizations",
                newName: "order_item_customizations_id");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItemCustomizations_OrderItemId",
                table: "order_item_customizations",
                newName: "IX_order_item_customizations_order_item_id");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItemCustomizations_CustomizationValueId",
                table: "order_item_customizations",
                newName: "IX_order_item_customizations_customization_value_id");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItemCustomizations_CustomizationOptionId",
                table: "order_item_customizations",
                newName: "IX_order_item_customizations_customization_option_id");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "order_addresses",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "State",
                table: "order_addresses",
                newName: "state");

            migrationBuilder.RenameColumn(
                name: "Pincode",
                table: "order_addresses",
                newName: "pincode");

            migrationBuilder.RenameColumn(
                name: "Line2",
                table: "order_addresses",
                newName: "line2");

            migrationBuilder.RenameColumn(
                name: "Line1",
                table: "order_addresses",
                newName: "line1");

            migrationBuilder.RenameColumn(
                name: "Country",
                table: "order_addresses",
                newName: "country");

            migrationBuilder.RenameColumn(
                name: "City",
                table: "order_addresses",
                newName: "city");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "order_addresses",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CustomerOrderId",
                table: "order_addresses",
                newName: "customer_order_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "order_addresses",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "OrderAddressesId",
                table: "order_addresses",
                newName: "order_addresses_id");

            migrationBuilder.RenameIndex(
                name: "IX_OrderAddresses_CustomerOrderId",
                table: "order_addresses",
                newName: "IX_order_addresses_customer_order_id");

            migrationBuilder.RenameColumn(
                name: "PD",
                table: "customer_prescriptions",
                newName: "pd");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "customer_prescriptions",
                newName: "notes");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "customer_prescriptions",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "customer_prescriptions",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "RightSphere",
                table: "customer_prescriptions",
                newName: "right_sphere");

            migrationBuilder.RenameColumn(
                name: "RightCylinder",
                table: "customer_prescriptions",
                newName: "right_cylinder");

            migrationBuilder.RenameColumn(
                name: "RightAxis",
                table: "customer_prescriptions",
                newName: "right_axis");

            migrationBuilder.RenameColumn(
                name: "RightAdd",
                table: "customer_prescriptions",
                newName: "right_add");

            migrationBuilder.RenameColumn(
                name: "LeftSphere",
                table: "customer_prescriptions",
                newName: "left_sphere");

            migrationBuilder.RenameColumn(
                name: "LeftCylinder",
                table: "customer_prescriptions",
                newName: "left_cylinder");

            migrationBuilder.RenameColumn(
                name: "LeftAxis",
                table: "customer_prescriptions",
                newName: "left_axis");

            migrationBuilder.RenameColumn(
                name: "LeftAdd",
                table: "customer_prescriptions",
                newName: "left_add");

            migrationBuilder.RenameColumn(
                name: "CustomerOrderId",
                table: "customer_prescriptions",
                newName: "customer_order_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "customer_prescriptions",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CustomerPrescriptionsId",
                table: "customer_prescriptions",
                newName: "customer_prescriptions_id");

            migrationBuilder.RenameIndex(
                name: "IX_CustomerPrescriptions_UserId",
                table: "customer_prescriptions",
                newName: "IX_customer_prescriptions_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_CustomerPrescriptions_CustomerOrderId",
                table: "customer_prescriptions",
                newName: "IX_customer_prescriptions_customer_order_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_payments",
                table: "payments",
                column: "payments_id");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_orders_customer_order_id",
                table: "orders",
                column: "customer_order_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_orders",
                table: "orders",
                column: "orders_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_order_status_logs",
                table: "order_status_logs",
                column: "order_status_logs_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_order_items",
                table: "order_items",
                column: "order_items_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_order_item_customizations",
                table: "order_item_customizations",
                column: "order_item_customizations_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_order_addresses",
                table: "order_addresses",
                column: "order_addresses_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_customer_prescriptions",
                table: "customer_prescriptions",
                column: "customer_prescriptions_id");

            migrationBuilder.AddCheckConstraint(
                name: "CK_payments_amount_non_negative",
                table: "payments",
                sql: "amount >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_orders_total_amount_non_negative",
                table: "orders",
                sql: "total_amount >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_order_items_price_non_negative",
                table: "order_items",
                sql: "price >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_order_items_quantity_positive",
                table: "order_items",
                sql: "quantity > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_order_items_total_price_non_negative",
                table: "order_items",
                sql: "total_price >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_customer_prescriptions_left_axis_range",
                table: "customer_prescriptions",
                sql: "left_axis IS NULL OR (left_axis >= 0 AND left_axis <= 180)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_customer_prescriptions_pd_non_negative",
                table: "customer_prescriptions",
                sql: "pd IS NULL OR pd >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_customer_prescriptions_right_axis_range",
                table: "customer_prescriptions",
                sql: "right_axis IS NULL OR (right_axis >= 0 AND right_axis <= 180)");

            migrationBuilder.AddForeignKey(
                name: "FK_customer_prescriptions_Users_user_id",
                table: "customer_prescriptions",
                column: "user_id",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_customer_prescriptions_orders_customer_order_id",
                table: "customer_prescriptions",
                column: "customer_order_id",
                principalTable: "orders",
                principalColumn: "customer_order_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_order_addresses_orders_customer_order_id",
                table: "order_addresses",
                column: "customer_order_id",
                principalTable: "orders",
                principalColumn: "customer_order_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_order_item_customizations_CustomizationOptions_customizatio~",
                table: "order_item_customizations",
                column: "customization_option_id",
                principalTable: "CustomizationOptions",
                principalColumn: "CustomizationOptionId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_order_item_customizations_CustomizationValues_customization~",
                table: "order_item_customizations",
                column: "customization_value_id",
                principalTable: "CustomizationValues",
                principalColumn: "CustomizationValueId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_order_item_customizations_order_items_order_item_id",
                table: "order_item_customizations",
                column: "order_item_id",
                principalTable: "order_items",
                principalColumn: "order_items_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_order_items_Products_product_id",
                table: "order_items",
                column: "product_id",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_order_items_orders_customer_order_id",
                table: "order_items",
                column: "customer_order_id",
                principalTable: "orders",
                principalColumn: "customer_order_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_order_status_logs_orders_customer_order_id",
                table: "order_status_logs",
                column: "customer_order_id",
                principalTable: "orders",
                principalColumn: "customer_order_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_orders_Users_user_id",
                table: "orders",
                column: "user_id",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_payments_orders_customer_order_id",
                table: "payments",
                column: "customer_order_id",
                principalTable: "orders",
                principalColumn: "customer_order_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
