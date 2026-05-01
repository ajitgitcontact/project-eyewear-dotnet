using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddCartAndWishlist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Carts",
                columns: table => new
                {
                    CartId = table.Column<string>(type: "varchar", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CartStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CustomerOrderId = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    CheckedOutAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AbandonedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carts", x => x.CartId);
                    table.ForeignKey(
                        name: "FK_Carts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Wishlists",
                columns: table => new
                {
                    WishlistId = table.Column<string>(type: "varchar", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wishlists", x => x.WishlistId);
                    table.ForeignKey(
                        name: "FK_Wishlists_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CartCoupons",
                columns: table => new
                {
                    CartCouponId = table.Column<string>(type: "varchar", nullable: false),
                    CartId = table.Column<string>(type: "varchar", nullable: false),
                    CouponId = table.Column<string>(type: "varchar", nullable: false),
                    CouponCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CouponDiscountAmount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartCoupons", x => x.CartCouponId);
                    table.CheckConstraint("CK_CartCoupons_CouponDiscountAmount_NonNegative", "\"CouponDiscountAmount\" >= 0");
                    table.ForeignKey(
                        name: "FK_CartCoupons_Carts_CartId",
                        column: x => x.CartId,
                        principalTable: "Carts",
                        principalColumn: "CartId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CartCoupons_Coupons_CouponId",
                        column: x => x.CouponId,
                        principalTable: "Coupons",
                        principalColumn: "CouponId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CartItems",
                columns: table => new
                {
                    CartItemId = table.Column<string>(type: "varchar", nullable: false),
                    CartId = table.Column<string>(type: "varchar", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    SKU = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    ProductDiscountAmount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    FinalUnitPrice = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItems", x => x.CartItemId);
                    table.CheckConstraint("CK_CartItems_FinalUnitPrice_NonNegative", "\"FinalUnitPrice\" >= 0");
                    table.CheckConstraint("CK_CartItems_LineTotal_NonNegative", "\"LineTotal\" >= 0");
                    table.CheckConstraint("CK_CartItems_ProductDiscountAmount_NonNegative", "\"ProductDiscountAmount\" >= 0");
                    table.CheckConstraint("CK_CartItems_Quantity_Positive", "\"Quantity\" > 0");
                    table.CheckConstraint("CK_CartItems_UnitPrice_NonNegative", "\"UnitPrice\" >= 0");
                    table.ForeignKey(
                        name: "FK_CartItems_Carts_CartId",
                        column: x => x.CartId,
                        principalTable: "Carts",
                        principalColumn: "CartId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CartItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WishlistItems",
                columns: table => new
                {
                    WishlistItemId = table.Column<string>(type: "varchar", nullable: false),
                    WishlistId = table.Column<string>(type: "varchar", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WishlistItems", x => x.WishlistItemId);
                    table.ForeignKey(
                        name: "FK_WishlistItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WishlistItems_Wishlists_WishlistId",
                        column: x => x.WishlistId,
                        principalTable: "Wishlists",
                        principalColumn: "WishlistId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CartItemCustomizations",
                columns: table => new
                {
                    CartItemCustomizationId = table.Column<string>(type: "varchar", nullable: false),
                    CartItemId = table.Column<string>(type: "varchar", nullable: false),
                    CustomizationOptionId = table.Column<int>(type: "integer", nullable: true),
                    CustomizationValueId = table.Column<int>(type: "integer", nullable: true),
                    CustomizationName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CustomizationValue = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExtraPrice = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItemCustomizations", x => x.CartItemCustomizationId);
                    table.ForeignKey(
                        name: "FK_CartItemCustomizations_CartItems_CartItemId",
                        column: x => x.CartItemId,
                        principalTable: "CartItems",
                        principalColumn: "CartItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CartItemCustomizations_CustomizationOptions_CustomizationOp~",
                        column: x => x.CustomizationOptionId,
                        principalTable: "CustomizationOptions",
                        principalColumn: "CustomizationOptionId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CartItemCustomizations_CustomizationValues_CustomizationVal~",
                        column: x => x.CustomizationValueId,
                        principalTable: "CustomizationValues",
                        principalColumn: "CustomizationValueId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CartItemPrescriptions",
                columns: table => new
                {
                    CartItemPrescriptionId = table.Column<string>(type: "varchar", nullable: false),
                    CartItemId = table.Column<string>(type: "varchar", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CustomerPrescriptionsId = table.Column<string>(type: "varchar", nullable: true),
                    RightSphere = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    RightCylinder = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    RightAxis = table.Column<int>(type: "integer", nullable: true),
                    RightAdd = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    LeftSphere = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    LeftCylinder = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    LeftAxis = table.Column<int>(type: "integer", nullable: true),
                    LeftAdd = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    PD = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItemPrescriptions", x => x.CartItemPrescriptionId);
                    table.ForeignKey(
                        name: "FK_CartItemPrescriptions_CartItems_CartItemId",
                        column: x => x.CartItemId,
                        principalTable: "CartItems",
                        principalColumn: "CartItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CartItemPrescriptions_CustomerPrescriptions_CustomerPrescri~",
                        column: x => x.CustomerPrescriptionsId,
                        principalTable: "CustomerPrescriptions",
                        principalColumn: "CustomerPrescriptionsId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CartItemPrescriptions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CartCoupons_CartId",
                table: "CartCoupons",
                column: "CartId");

            migrationBuilder.CreateIndex(
                name: "IX_CartCoupons_CouponId",
                table: "CartCoupons",
                column: "CouponId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItemCustomizations_CartItemId",
                table: "CartItemCustomizations",
                column: "CartItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItemCustomizations_CustomizationOptionId",
                table: "CartItemCustomizations",
                column: "CustomizationOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItemCustomizations_CustomizationValueId",
                table: "CartItemCustomizations",
                column: "CustomizationValueId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItemPrescriptions_CartItemId",
                table: "CartItemPrescriptions",
                column: "CartItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CartItemPrescriptions_CustomerPrescriptionsId",
                table: "CartItemPrescriptions",
                column: "CustomerPrescriptionsId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItemPrescriptions_UserId",
                table: "CartItemPrescriptions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId",
                table: "CartItems",
                column: "CartId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_ProductId",
                table: "CartItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_UserId",
                table: "Carts",
                column: "UserId",
                unique: true,
                filter: "\"CartStatus\" = 'ACTIVE'");

            migrationBuilder.CreateIndex(
                name: "IX_WishlistItems_ProductId",
                table: "WishlistItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_WishlistItems_WishlistId_ProductId",
                table: "WishlistItems",
                columns: new[] { "WishlistId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wishlists_UserId",
                table: "Wishlists",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CartCoupons");

            migrationBuilder.DropTable(
                name: "CartItemCustomizations");

            migrationBuilder.DropTable(
                name: "CartItemPrescriptions");

            migrationBuilder.DropTable(
                name: "WishlistItems");

            migrationBuilder.DropTable(
                name: "CartItems");

            migrationBuilder.DropTable(
                name: "Wishlists");

            migrationBuilder.DropTable(
                name: "Carts");
        }
    }
}
