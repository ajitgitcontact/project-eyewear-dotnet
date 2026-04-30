using System;
using backend.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260501000000_AddOrderNumberSequencesAndRelaxOrderItemSkuIndex")]
    public partial class AddOrderNumberSequencesAndRelaxOrderItemSkuIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrderItems_SKU",
                table: "OrderItems");

            migrationBuilder.CreateTable(
                name: "OrderNumberSequences",
                columns: table => new
                {
                    OrderNumberSequencesId = table.Column<string>(type: "varchar", nullable: false),
                    SequenceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    LastSequenceNumber = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderNumberSequences", x => x.OrderNumberSequencesId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_SKU",
                table: "OrderItems",
                column: "SKU");

            migrationBuilder.CreateIndex(
                name: "IX_OrderNumberSequences_SequenceDate",
                table: "OrderNumberSequences",
                column: "SequenceDate",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderNumberSequences");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_SKU",
                table: "OrderItems");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_SKU",
                table: "OrderItems",
                column: "SKU",
                unique: true);
        }
    }
}
