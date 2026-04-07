using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddProductEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SKU = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Brand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BasePrice = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    HasPrescription = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductId);
                });

            migrationBuilder.CreateTable(
                name: "CustomizationOptions",
                columns: table => new
                {
                    CustomizationOptionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomizationOptions", x => x.CustomizationOptionId);
                    table.ForeignKey(
                        name: "FK_CustomizationOptions_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductImages",
                columns: table => new
                {
                    ProductImageId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductImages", x => x.ProductImageId);
                    table.ForeignKey(
                        name: "FK_ProductImages_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomizationValues",
                columns: table => new
                {
                    CustomizationValueId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomizationOptionId = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AdditionalPrice = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomizationValues", x => x.CustomizationValueId);
                    table.ForeignKey(
                        name: "FK_CustomizationValues_CustomizationOptions_CustomizationOptio~",
                        column: x => x.CustomizationOptionId,
                        principalTable: "CustomizationOptions",
                        principalColumn: "CustomizationOptionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomizationImages",
                columns: table => new
                {
                    CustomizationImageId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    CustomizationOptionId = table.Column<int>(type: "integer", nullable: false),
                    CustomizationValueId = table.Column<int>(type: "integer", nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomizationImages", x => x.CustomizationImageId);
                    table.ForeignKey(
                        name: "FK_CustomizationImages_CustomizationOptions_CustomizationOptio~",
                        column: x => x.CustomizationOptionId,
                        principalTable: "CustomizationOptions",
                        principalColumn: "CustomizationOptionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomizationImages_CustomizationValues_CustomizationValueId",
                        column: x => x.CustomizationValueId,
                        principalTable: "CustomizationValues",
                        principalColumn: "CustomizationValueId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomizationImages_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomizationImages_CustomizationOptionId",
                table: "CustomizationImages",
                column: "CustomizationOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomizationImages_CustomizationValueId",
                table: "CustomizationImages",
                column: "CustomizationValueId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomizationImages_ProductId",
                table: "CustomizationImages",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomizationOptions_ProductId",
                table: "CustomizationOptions",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomizationValues_CustomizationOptionId",
                table: "CustomizationValues",
                column: "CustomizationOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId",
                table: "ProductImages",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_SKU",
                table: "Products",
                column: "SKU",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomizationImages");

            migrationBuilder.DropTable(
                name: "ProductImages");

            migrationBuilder.DropTable(
                name: "CustomizationValues");

            migrationBuilder.DropTable(
                name: "CustomizationOptions");

            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
