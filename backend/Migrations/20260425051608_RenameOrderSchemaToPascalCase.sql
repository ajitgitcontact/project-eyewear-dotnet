START TRANSACTION;
ALTER TABLE customer_prescriptions DROP CONSTRAINT "FK_customer_prescriptions_Users_user_id";

ALTER TABLE customer_prescriptions DROP CONSTRAINT "FK_customer_prescriptions_orders_customer_order_id";

ALTER TABLE order_addresses DROP CONSTRAINT "FK_order_addresses_orders_customer_order_id";

ALTER TABLE order_item_customizations DROP CONSTRAINT "FK_order_item_customizations_CustomizationOptions_customizatio~";

ALTER TABLE order_item_customizations DROP CONSTRAINT "FK_order_item_customizations_CustomizationValues_customization~";

ALTER TABLE order_item_customizations DROP CONSTRAINT "FK_order_item_customizations_order_items_order_item_id";

ALTER TABLE order_items DROP CONSTRAINT "FK_order_items_Products_product_id";

ALTER TABLE order_items DROP CONSTRAINT "FK_order_items_orders_customer_order_id";

ALTER TABLE order_status_logs DROP CONSTRAINT "FK_order_status_logs_orders_customer_order_id";

ALTER TABLE orders DROP CONSTRAINT "FK_orders_Users_user_id";

ALTER TABLE payments DROP CONSTRAINT "FK_payments_orders_customer_order_id";

ALTER TABLE payments DROP CONSTRAINT "PK_payments";

ALTER TABLE payments DROP CONSTRAINT "CK_payments_amount_non_negative";

ALTER TABLE orders DROP CONSTRAINT "AK_orders_customer_order_id";

ALTER TABLE orders DROP CONSTRAINT "PK_orders";

ALTER TABLE orders DROP CONSTRAINT "CK_orders_total_amount_non_negative";

ALTER TABLE order_status_logs DROP CONSTRAINT "PK_order_status_logs";

ALTER TABLE order_items DROP CONSTRAINT "PK_order_items";

ALTER TABLE order_items DROP CONSTRAINT "CK_order_items_price_non_negative";

ALTER TABLE order_items DROP CONSTRAINT "CK_order_items_quantity_positive";

ALTER TABLE order_items DROP CONSTRAINT "CK_order_items_total_price_non_negative";

ALTER TABLE order_item_customizations DROP CONSTRAINT "PK_order_item_customizations";

ALTER TABLE order_addresses DROP CONSTRAINT "PK_order_addresses";

ALTER TABLE customer_prescriptions DROP CONSTRAINT "PK_customer_prescriptions";

ALTER TABLE customer_prescriptions DROP CONSTRAINT "CK_customer_prescriptions_left_axis_range";

ALTER TABLE customer_prescriptions DROP CONSTRAINT "CK_customer_prescriptions_pd_non_negative";

ALTER TABLE customer_prescriptions DROP CONSTRAINT "CK_customer_prescriptions_right_axis_range";

ALTER TABLE payments RENAME TO "Payments";

ALTER TABLE orders RENAME TO "Orders";

ALTER TABLE order_status_logs RENAME TO "OrderStatusLogs";

ALTER TABLE order_items RENAME TO "OrderItems";

ALTER TABLE order_item_customizations RENAME TO "OrderItemCustomizations";

ALTER TABLE order_addresses RENAME TO "OrderAddresses";

ALTER TABLE customer_prescriptions RENAME TO "CustomerPrescriptions";

ALTER TABLE "Payments" RENAME COLUMN status TO "Status";

ALTER TABLE "Payments" RENAME COLUMN method TO "Method";

ALTER TABLE "Payments" RENAME COLUMN amount TO "Amount";

ALTER TABLE "Payments" RENAME COLUMN updated_at TO "UpdatedAt";

ALTER TABLE "Payments" RENAME COLUMN transaction_id TO "TransactionId";

ALTER TABLE "Payments" RENAME COLUMN customer_order_id TO "CustomerOrderId";

ALTER TABLE "Payments" RENAME COLUMN created_at TO "CreatedAt";

ALTER TABLE "Payments" RENAME COLUMN payments_id TO "PaymentsId";

ALTER INDEX "IX_payments_customer_order_id" RENAME TO "IX_Payments_CustomerOrderId";

ALTER TABLE "Orders" RENAME COLUMN notes TO "Notes";

ALTER TABLE "Orders" RENAME COLUMN user_id TO "UserId";

ALTER TABLE "Orders" RENAME COLUMN updated_at TO "UpdatedAt";

ALTER TABLE "Orders" RENAME COLUMN total_amount TO "TotalAmount";

ALTER TABLE "Orders" RENAME COLUMN payment_status TO "PaymentStatus";

ALTER TABLE "Orders" RENAME COLUMN order_status TO "OrderStatus";

ALTER TABLE "Orders" RENAME COLUMN customer_phone TO "CustomerPhone";

ALTER TABLE "Orders" RENAME COLUMN customer_order_id TO "CustomerOrderId";

ALTER TABLE "Orders" RENAME COLUMN customer_name TO "CustomerName";

ALTER TABLE "Orders" RENAME COLUMN customer_email TO "CustomerEmail";

ALTER TABLE "Orders" RENAME COLUMN created_at TO "CreatedAt";

ALTER TABLE "Orders" RENAME COLUMN orders_id TO "OrdersId";

ALTER INDEX "IX_orders_user_id" RENAME TO "IX_Orders_UserId";

ALTER INDEX "IX_orders_customer_order_id" RENAME TO "IX_Orders_CustomerOrderId";

ALTER TABLE "OrderStatusLogs" RENAME COLUMN status TO "Status";

ALTER TABLE "OrderStatusLogs" RENAME COLUMN comment TO "Comment";

ALTER TABLE "OrderStatusLogs" RENAME COLUMN updated_at TO "UpdatedAt";

ALTER TABLE "OrderStatusLogs" RENAME COLUMN customer_order_id TO "CustomerOrderId";

ALTER TABLE "OrderStatusLogs" RENAME COLUMN created_at TO "CreatedAt";

ALTER TABLE "OrderStatusLogs" RENAME COLUMN order_status_logs_id TO "OrderStatusLogsId";

ALTER INDEX "IX_order_status_logs_customer_order_id" RENAME TO "IX_OrderStatusLogs_CustomerOrderId";

ALTER TABLE "OrderItems" RENAME COLUMN sku TO "SKU";

ALTER TABLE "OrderItems" RENAME COLUMN quantity TO "Quantity";

ALTER TABLE "OrderItems" RENAME COLUMN price TO "Price";

ALTER TABLE "OrderItems" RENAME COLUMN updated_at TO "UpdatedAt";

ALTER TABLE "OrderItems" RENAME COLUMN total_price TO "TotalPrice";

ALTER TABLE "OrderItems" RENAME COLUMN product_name TO "ProductName";

ALTER TABLE "OrderItems" RENAME COLUMN product_id TO "ProductId";

ALTER TABLE "OrderItems" RENAME COLUMN customer_order_id TO "CustomerOrderId";

ALTER TABLE "OrderItems" RENAME COLUMN created_at TO "CreatedAt";

ALTER TABLE "OrderItems" RENAME COLUMN order_items_id TO "OrderItemsId";

ALTER INDEX "IX_order_items_sku" RENAME TO "IX_OrderItems_SKU";

ALTER INDEX "IX_order_items_product_id" RENAME TO "IX_OrderItems_ProductId";

ALTER INDEX "IX_order_items_customer_order_id" RENAME TO "IX_OrderItems_CustomerOrderId";

ALTER TABLE "OrderItemCustomizations" RENAME COLUMN value TO "Value";

ALTER TABLE "OrderItemCustomizations" RENAME COLUMN type TO "Type";

ALTER TABLE "OrderItemCustomizations" RENAME COLUMN updated_at TO "UpdatedAt";

ALTER TABLE "OrderItemCustomizations" RENAME COLUMN order_item_id TO "OrderItemId";

ALTER TABLE "OrderItemCustomizations" RENAME COLUMN customization_value_id TO "CustomizationValueId";

ALTER TABLE "OrderItemCustomizations" RENAME COLUMN customization_option_id TO "CustomizationOptionId";

ALTER TABLE "OrderItemCustomizations" RENAME COLUMN created_at TO "CreatedAt";

ALTER TABLE "OrderItemCustomizations" RENAME COLUMN order_item_customizations_id TO "OrderItemCustomizationsId";

ALTER INDEX "IX_order_item_customizations_order_item_id" RENAME TO "IX_OrderItemCustomizations_OrderItemId";

ALTER INDEX "IX_order_item_customizations_customization_value_id" RENAME TO "IX_OrderItemCustomizations_CustomizationValueId";

ALTER INDEX "IX_order_item_customizations_customization_option_id" RENAME TO "IX_OrderItemCustomizations_CustomizationOptionId";

ALTER TABLE "OrderAddresses" RENAME COLUMN type TO "Type";

ALTER TABLE "OrderAddresses" RENAME COLUMN state TO "State";

ALTER TABLE "OrderAddresses" RENAME COLUMN pincode TO "Pincode";

ALTER TABLE "OrderAddresses" RENAME COLUMN line2 TO "Line2";

ALTER TABLE "OrderAddresses" RENAME COLUMN line1 TO "Line1";

ALTER TABLE "OrderAddresses" RENAME COLUMN country TO "Country";

ALTER TABLE "OrderAddresses" RENAME COLUMN city TO "City";

ALTER TABLE "OrderAddresses" RENAME COLUMN updated_at TO "UpdatedAt";

ALTER TABLE "OrderAddresses" RENAME COLUMN customer_order_id TO "CustomerOrderId";

ALTER TABLE "OrderAddresses" RENAME COLUMN created_at TO "CreatedAt";

ALTER TABLE "OrderAddresses" RENAME COLUMN order_addresses_id TO "OrderAddressesId";

ALTER INDEX "IX_order_addresses_customer_order_id" RENAME TO "IX_OrderAddresses_CustomerOrderId";

ALTER TABLE "CustomerPrescriptions" RENAME COLUMN pd TO "PD";

ALTER TABLE "CustomerPrescriptions" RENAME COLUMN notes TO "Notes";

ALTER TABLE "CustomerPrescriptions" RENAME COLUMN user_id TO "UserId";

ALTER TABLE "CustomerPrescriptions" RENAME COLUMN updated_at TO "UpdatedAt";

ALTER TABLE "CustomerPrescriptions" RENAME COLUMN right_sphere TO "RightSphere";

ALTER TABLE "CustomerPrescriptions" RENAME COLUMN right_cylinder TO "RightCylinder";

ALTER TABLE "CustomerPrescriptions" RENAME COLUMN right_axis TO "RightAxis";

ALTER TABLE "CustomerPrescriptions" RENAME COLUMN right_add TO "RightAdd";

ALTER TABLE "CustomerPrescriptions" RENAME COLUMN left_sphere TO "LeftSphere";

ALTER TABLE "CustomerPrescriptions" RENAME COLUMN left_cylinder TO "LeftCylinder";

ALTER TABLE "CustomerPrescriptions" RENAME COLUMN left_axis TO "LeftAxis";

ALTER TABLE "CustomerPrescriptions" RENAME COLUMN left_add TO "LeftAdd";

ALTER TABLE "CustomerPrescriptions" RENAME COLUMN customer_order_id TO "CustomerOrderId";

ALTER TABLE "CustomerPrescriptions" RENAME COLUMN created_at TO "CreatedAt";

ALTER TABLE "CustomerPrescriptions" RENAME COLUMN customer_prescriptions_id TO "CustomerPrescriptionsId";

ALTER INDEX "IX_customer_prescriptions_user_id" RENAME TO "IX_CustomerPrescriptions_UserId";

ALTER INDEX "IX_customer_prescriptions_customer_order_id" RENAME TO "IX_CustomerPrescriptions_CustomerOrderId";

ALTER TABLE "Payments" ADD CONSTRAINT "PK_Payments" PRIMARY KEY ("PaymentsId");

ALTER TABLE "Orders" ADD CONSTRAINT "AK_Orders_CustomerOrderId" UNIQUE ("CustomerOrderId");

ALTER TABLE "Orders" ADD CONSTRAINT "PK_Orders" PRIMARY KEY ("OrdersId");

ALTER TABLE "OrderStatusLogs" ADD CONSTRAINT "PK_OrderStatusLogs" PRIMARY KEY ("OrderStatusLogsId");

ALTER TABLE "OrderItems" ADD CONSTRAINT "PK_OrderItems" PRIMARY KEY ("OrderItemsId");

ALTER TABLE "OrderItemCustomizations" ADD CONSTRAINT "PK_OrderItemCustomizations" PRIMARY KEY ("OrderItemCustomizationsId");

ALTER TABLE "OrderAddresses" ADD CONSTRAINT "PK_OrderAddresses" PRIMARY KEY ("OrderAddressesId");

ALTER TABLE "CustomerPrescriptions" ADD CONSTRAINT "PK_CustomerPrescriptions" PRIMARY KEY ("CustomerPrescriptionsId");

ALTER TABLE "Payments" ADD CONSTRAINT "CK_Payments_Amount_NonNegative" CHECK ("Amount" >= 0);

ALTER TABLE "Orders" ADD CONSTRAINT "CK_Orders_TotalAmount_NonNegative" CHECK ("TotalAmount" >= 0);

ALTER TABLE "OrderItems" ADD CONSTRAINT "CK_OrderItems_Price_NonNegative" CHECK ("Price" >= 0);

ALTER TABLE "OrderItems" ADD CONSTRAINT "CK_OrderItems_Quantity_Positive" CHECK ("Quantity" > 0);

ALTER TABLE "OrderItems" ADD CONSTRAINT "CK_OrderItems_TotalPrice_NonNegative" CHECK ("TotalPrice" >= 0);

ALTER TABLE "CustomerPrescriptions" ADD CONSTRAINT "CK_CustomerPrescriptions_LeftAxis_Range" CHECK ("LeftAxis" IS NULL OR ("LeftAxis" >= 0 AND "LeftAxis" <= 180));

ALTER TABLE "CustomerPrescriptions" ADD CONSTRAINT "CK_CustomerPrescriptions_PD_NonNegative" CHECK ("PD" IS NULL OR "PD" >= 0);

ALTER TABLE "CustomerPrescriptions" ADD CONSTRAINT "CK_CustomerPrescriptions_RightAxis_Range" CHECK ("RightAxis" IS NULL OR ("RightAxis" >= 0 AND "RightAxis" <= 180));

ALTER TABLE "CustomerPrescriptions" ADD CONSTRAINT "FK_CustomerPrescriptions_Orders_CustomerOrderId" FOREIGN KEY ("CustomerOrderId") REFERENCES "Orders" ("CustomerOrderId") ON DELETE CASCADE;

ALTER TABLE "CustomerPrescriptions" ADD CONSTRAINT "FK_CustomerPrescriptions_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT;

ALTER TABLE "OrderAddresses" ADD CONSTRAINT "FK_OrderAddresses_Orders_CustomerOrderId" FOREIGN KEY ("CustomerOrderId") REFERENCES "Orders" ("CustomerOrderId") ON DELETE CASCADE;

ALTER TABLE "OrderItemCustomizations" ADD CONSTRAINT "FK_OrderItemCustomizations_CustomizationOptions_CustomizationO~" FOREIGN KEY ("CustomizationOptionId") REFERENCES "CustomizationOptions" ("CustomizationOptionId") ON DELETE SET NULL;

ALTER TABLE "OrderItemCustomizations" ADD CONSTRAINT "FK_OrderItemCustomizations_CustomizationValues_CustomizationVa~" FOREIGN KEY ("CustomizationValueId") REFERENCES "CustomizationValues" ("CustomizationValueId") ON DELETE SET NULL;

ALTER TABLE "OrderItemCustomizations" ADD CONSTRAINT "FK_OrderItemCustomizations_OrderItems_OrderItemId" FOREIGN KEY ("OrderItemId") REFERENCES "OrderItems" ("OrderItemsId") ON DELETE CASCADE;

ALTER TABLE "OrderItems" ADD CONSTRAINT "FK_OrderItems_Orders_CustomerOrderId" FOREIGN KEY ("CustomerOrderId") REFERENCES "Orders" ("CustomerOrderId") ON DELETE CASCADE;

ALTER TABLE "OrderItems" ADD CONSTRAINT "FK_OrderItems_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("ProductId") ON DELETE RESTRICT;

ALTER TABLE "Orders" ADD CONSTRAINT "FK_Orders_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT;

ALTER TABLE "OrderStatusLogs" ADD CONSTRAINT "FK_OrderStatusLogs_Orders_CustomerOrderId" FOREIGN KEY ("CustomerOrderId") REFERENCES "Orders" ("CustomerOrderId") ON DELETE CASCADE;

ALTER TABLE "Payments" ADD CONSTRAINT "FK_Payments_Orders_CustomerOrderId" FOREIGN KEY ("CustomerOrderId") REFERENCES "Orders" ("CustomerOrderId") ON DELETE CASCADE;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260425051608_RenameOrderSchemaToPascalCase', '9.0.15');

COMMIT;

