using BookingHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BookingHub.Infrastructure.Persistence.Migrations;

[DbContext(typeof(BookingHubDbContext))]
[Migration("20260919220000_AddInboxProcessing")]
public sealed class AddInboxProcessing : Migration
{
    protected override void Up(
        MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "inbox_messages",
            columns: table => new
            {
                Consumer = table.Column<string>(
                    type: "character varying(200)",
                    maxLength: 200,
                    nullable: false),
                MessageId = table.Column<Guid>(
                    type: "uuid",
                    nullable: false),
                ProcessedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey(
                    "PK_inbox_messages",
                    x => new
                    {
                        x.Consumer,
                        x.MessageId
                    });
            });

        migrationBuilder.CreateIndex(
            name: "IX_inbox_messages_ProcessedAtUtc",
            table: "inbox_messages",
            column: "ProcessedAtUtc");
    }

    protected override void Down(
        MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "inbox_messages");
    }
}
