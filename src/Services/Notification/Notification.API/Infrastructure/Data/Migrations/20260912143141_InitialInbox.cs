using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notification.API.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialInbox : Migration
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inbox_messages",
                schema: "messaging");
        }
    }
}
