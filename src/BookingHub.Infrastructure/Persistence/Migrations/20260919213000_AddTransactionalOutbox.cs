using BookingHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BookingHub.Infrastructure.Persistence.Migrations;

[DbContext(typeof(BookingHubDbContext))]
[Migration("20260919213000_AddTransactionalOutbox")]
public sealed class AddTransactionalOutbox : Migration
{
    protected override void Up(
        MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "outbox_messages",
            columns: table => new
            {
                Id = table.Column<Guid>(
                    type: "uuid",
                    nullable: false),
                Type = table.Column<string>(
                    type: "character varying(200)",
                    maxLength: 200,
                    nullable: false),
                Payload = table.Column<string>(
                    type: "jsonb",
                    nullable: false),
                OccurredAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false),
                ProcessedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: true),
                AttemptCount = table.Column<int>(
                    type: "integer",
                    nullable: false),
                LastError = table.Column<string>(
                    type: "character varying(2000)",
                    maxLength: 2000,
                    nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey(
                    "PK_outbox_messages",
                    x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_outbox_messages_OccurredAtUtc",
            table: "outbox_messages",
            column: "OccurredAtUtc");

        migrationBuilder.CreateIndex(
            name: "IX_outbox_messages_ProcessedAtUtc",
            table: "outbox_messages",
            column: "ProcessedAtUtc");
    }

    protected override void Down(
        MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "outbox_messages");
    }
}
