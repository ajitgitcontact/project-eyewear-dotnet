using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckoutIdempotencyAndCouponUsageGuards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdempotencyKey",
                table: "Orders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId_IdempotencyKey",
                table: "Orders",
                columns: new[] { "UserId", "IdempotencyKey" },
                unique: true,
                filter: "\"IdempotencyKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CouponUsages_CouponId_CustomerOrderId",
                table: "CouponUsages",
                columns: new[] { "CouponId", "CustomerOrderId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_UserId_IdempotencyKey",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_CouponUsages_CouponId_CustomerOrderId",
                table: "CouponUsages");

            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                table: "Orders");
        }
    }
}
