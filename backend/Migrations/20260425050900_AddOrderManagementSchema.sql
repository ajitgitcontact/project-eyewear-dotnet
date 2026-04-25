START TRANSACTION;
CREATE TYPE payment_status AS ENUM ('PENDING', 'PAID', 'FAILED');
CREATE TYPE order_status AS ENUM ('CREATED', 'CONFIRMED', 'SHIPPED', 'DELIVERED', 'CANCELLED');
CREATE TYPE payment_method AS ENUM ('COD', 'PREPAID', 'UPI', 'CARD');
CREATE TYPE payment_txn_status AS ENUM ('INITIATED', 'SUCCESS', 'FAILED');
CREATE TYPE address_type AS ENUM ('SHIPPING', 'BILLING');

CREATE TABLE orders (
    orders_id varchar NOT NULL,
    customer_order_id varchar(50) NOT NULL,
    user_id integer NOT NULL,
    customer_name character varying(200) NOT NULL,
    customer_email character varying(150) NOT NULL,
    customer_phone character varying(20),
    total_amount numeric(10,2) NOT NULL,
    payment_status payment_status NOT NULL,
    order_status order_status NOT NULL,
    notes text,
    created_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updated_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    CONSTRAINT "PK_orders" PRIMARY KEY (orders_id),
    CONSTRAINT "AK_orders_customer_order_id" UNIQUE (customer_order_id),
    CONSTRAINT "CK_orders_total_amount_non_negative" CHECK (total_amount >= 0),
    CONSTRAINT "FK_orders_Users_user_id" FOREIGN KEY (user_id) REFERENCES "Users" ("Id") ON DELETE RESTRICT
);

CREATE TABLE customer_prescriptions (
    customer_prescriptions_id varchar NOT NULL,
    user_id integer NOT NULL,
    customer_order_id varchar(50) NOT NULL,
    right_sphere numeric(5,2),
    right_cylinder numeric(5,2),
    right_axis integer,
    right_add numeric(5,2),
    left_sphere numeric(5,2),
    left_cylinder numeric(5,2),
    left_axis integer,
    left_add numeric(5,2),
    pd numeric(5,2),
    notes text,
    created_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updated_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    CONSTRAINT "PK_customer_prescriptions" PRIMARY KEY (customer_prescriptions_id),
    CONSTRAINT "CK_customer_prescriptions_left_axis_range" CHECK (left_axis IS NULL OR (left_axis >= 0 AND left_axis <= 180)),
    CONSTRAINT "CK_customer_prescriptions_pd_non_negative" CHECK (pd IS NULL OR pd >= 0),
    CONSTRAINT "CK_customer_prescriptions_right_axis_range" CHECK (right_axis IS NULL OR (right_axis >= 0 AND right_axis <= 180)),
    CONSTRAINT "FK_customer_prescriptions_Users_user_id" FOREIGN KEY (user_id) REFERENCES "Users" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_customer_prescriptions_orders_customer_order_id" FOREIGN KEY (customer_order_id) REFERENCES orders (customer_order_id) ON DELETE CASCADE
);

CREATE TABLE order_addresses (
    order_addresses_id varchar NOT NULL,
    customer_order_id varchar(50) NOT NULL,
    type address_type NOT NULL,
    line1 character varying(250) NOT NULL,
    line2 character varying(250),
    city character varying(100) NOT NULL,
    state character varying(100) NOT NULL,
    pincode character varying(20) NOT NULL,
    country character varying(100) NOT NULL,
    created_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updated_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    CONSTRAINT "PK_order_addresses" PRIMARY KEY (order_addresses_id),
    CONSTRAINT "FK_order_addresses_orders_customer_order_id" FOREIGN KEY (customer_order_id) REFERENCES orders (customer_order_id) ON DELETE CASCADE
);

CREATE TABLE order_items (
    order_items_id varchar NOT NULL,
    customer_order_id varchar(50) NOT NULL,
    product_id integer NOT NULL,
    sku character varying(50) NOT NULL,
    product_name character varying(200) NOT NULL,
    quantity integer NOT NULL,
    price numeric(10,2) NOT NULL,
    total_price numeric(10,2) NOT NULL,
    created_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updated_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    CONSTRAINT "PK_order_items" PRIMARY KEY (order_items_id),
    CONSTRAINT "CK_order_items_price_non_negative" CHECK (price >= 0),
    CONSTRAINT "CK_order_items_quantity_positive" CHECK (quantity > 0),
    CONSTRAINT "CK_order_items_total_price_non_negative" CHECK (total_price >= 0),
    CONSTRAINT "FK_order_items_Products_product_id" FOREIGN KEY (product_id) REFERENCES "Products" ("ProductId") ON DELETE RESTRICT,
    CONSTRAINT "FK_order_items_orders_customer_order_id" FOREIGN KEY (customer_order_id) REFERENCES orders (customer_order_id) ON DELETE CASCADE
);

CREATE TABLE order_status_logs (
    order_status_logs_id varchar NOT NULL,
    customer_order_id varchar(50) NOT NULL,
    status order_status NOT NULL,
    comment text,
    created_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updated_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    CONSTRAINT "PK_order_status_logs" PRIMARY KEY (order_status_logs_id),
    CONSTRAINT "FK_order_status_logs_orders_customer_order_id" FOREIGN KEY (customer_order_id) REFERENCES orders (customer_order_id) ON DELETE CASCADE
);

CREATE TABLE payments (
    payments_id varchar NOT NULL,
    customer_order_id varchar(50) NOT NULL,
    method payment_method NOT NULL,
    transaction_id character varying(150),
    amount numeric(10,2) NOT NULL,
    status payment_txn_status NOT NULL,
    created_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updated_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    CONSTRAINT "PK_payments" PRIMARY KEY (payments_id),
    CONSTRAINT "CK_payments_amount_non_negative" CHECK (amount >= 0),
    CONSTRAINT "FK_payments_orders_customer_order_id" FOREIGN KEY (customer_order_id) REFERENCES orders (customer_order_id) ON DELETE CASCADE
);

CREATE TABLE order_item_customizations (
    order_item_customizations_id varchar NOT NULL,
    order_item_id varchar NOT NULL,
    customization_option_id integer,
    customization_value_id integer,
    type character varying(100) NOT NULL,
    value character varying(100) NOT NULL,
    created_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updated_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    CONSTRAINT "PK_order_item_customizations" PRIMARY KEY (order_item_customizations_id),
    CONSTRAINT "FK_order_item_customizations_CustomizationOptions_customizatio~" FOREIGN KEY (customization_option_id) REFERENCES "CustomizationOptions" ("CustomizationOptionId") ON DELETE SET NULL,
    CONSTRAINT "FK_order_item_customizations_CustomizationValues_customization~" FOREIGN KEY (customization_value_id) REFERENCES "CustomizationValues" ("CustomizationValueId") ON DELETE SET NULL,
    CONSTRAINT "FK_order_item_customizations_order_items_order_item_id" FOREIGN KEY (order_item_id) REFERENCES order_items (order_items_id) ON DELETE CASCADE
);

CREATE INDEX "IX_customer_prescriptions_customer_order_id" ON customer_prescriptions (customer_order_id);

CREATE INDEX "IX_customer_prescriptions_user_id" ON customer_prescriptions (user_id);

CREATE INDEX "IX_order_addresses_customer_order_id" ON order_addresses (customer_order_id);

CREATE INDEX "IX_order_item_customizations_customization_option_id" ON order_item_customizations (customization_option_id);

CREATE INDEX "IX_order_item_customizations_customization_value_id" ON order_item_customizations (customization_value_id);

CREATE INDEX "IX_order_item_customizations_order_item_id" ON order_item_customizations (order_item_id);

CREATE INDEX "IX_order_items_customer_order_id" ON order_items (customer_order_id);

CREATE INDEX "IX_order_items_product_id" ON order_items (product_id);

CREATE UNIQUE INDEX "IX_order_items_sku" ON order_items (sku);

CREATE INDEX "IX_order_status_logs_customer_order_id" ON order_status_logs (customer_order_id);

CREATE UNIQUE INDEX "IX_orders_customer_order_id" ON orders (customer_order_id);

CREATE INDEX "IX_orders_user_id" ON orders (user_id);

CREATE INDEX "IX_payments_customer_order_id" ON payments (customer_order_id);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260425050900_AddOrderManagementSchema', '9.0.15');

COMMIT;

