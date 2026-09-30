using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ordering.API.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCardInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "payment_card_name",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "payment_card_number",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "payment_cvv",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "payment_expiration",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "payment_payment_method",
                table: "orders");

            migrationBuilder.AddColumn<string>(
                name: "payment_payment_token",
                table: "orders",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "payment_payment_token",
                table: "orders");

            migrationBuilder.AddColumn<string>(
                name: "payment_card_name",
                table: "orders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "payment_card_number",
                table: "orders",
                type: "character varying(25)",
                maxLength: 25,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "payment_cvv",
                table: "orders",
                type: "character varying(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "payment_expiration",
                table: "orders",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "payment_payment_method",
                table: "orders",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
