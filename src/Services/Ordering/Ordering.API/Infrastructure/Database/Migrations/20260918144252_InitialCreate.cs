using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ordering.API.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "messaging");

            migrationBuilder.CreateTable(
                name: "inbox_messages",
                schema: "messaging",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    content = table.Column<string>(type: "jsonb", nullable: false),
                    partition_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    occurred_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    received_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    next_attempt_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    locked_until_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error = table.Column<string>(type: "text", nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    total_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    billing_address_address_line = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    billing_address_country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    billing_address_email_address = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    billing_address_first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    billing_address_last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    billing_address_state = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    billing_address_zip_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    payment_cvv = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    payment_card_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    payment_card_number = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    payment_expiration = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    payment_payment_method = table.Column<int>(type: "integer", nullable: false),
                    shipping_address_address_line = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    shipping_address_country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    shipping_address_email_address = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    shipping_address_first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    shipping_address_last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    shipping_address_state = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    shipping_address_zip_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_orders", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "messaging",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    partition_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    occurred_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    locked_until_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    next_attempt_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error = table.Column<string>(type: "text", nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    trace_parent = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "order_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_order_items_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_inbox_messages_occurred_on_utc",
                schema: "messaging",
                table: "inbox_messages",
                column: "occurred_on_utc",
                filter: "\"status\" = 0");

            migrationBuilder.CreateIndex(
                name: "ix_inbox_messages_partition_key_occurred_on_utc",
                schema: "messaging",
                table: "inbox_messages",
                columns: new[] { "partition_key", "occurred_on_utc" },
                filter: "\"status\" = 0");

            migrationBuilder.CreateIndex(
                name: "ix_inbox_messages_processed_on_utc",
                schema: "messaging",
                table: "inbox_messages",
                column: "processed_on_utc",
                filter: "\"status\" = 1");

            migrationBuilder.CreateIndex(
                name: "ix_order_items_order_id",
                table: "order_items",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_partition_pending",
                schema: "messaging",
                table: "outbox_messages",
                columns: new[] { "partition_key", "occurred_on_utc" },
                filter: "\"status\" = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inbox_messages",
                schema: "messaging");

            migrationBuilder.DropTable(
                name: "order_items");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "messaging");

            migrationBuilder.DropTable(
                name: "orders");
        }
    }
}
