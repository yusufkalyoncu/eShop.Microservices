using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ordering.Saga.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddItemsToOrderState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "items",
                table: "order_states",
                type: "jsonb",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "items",
                table: "order_states");
        }
    }
}
