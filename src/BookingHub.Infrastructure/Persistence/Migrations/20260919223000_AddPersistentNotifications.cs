using BookingHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BookingHub.Infrastructure.Persistence.Migrations;

[DbContext(typeof(BookingHubDbContext))]
[Migration("20260919223000_AddPersistentNotifications")]
public sealed class AddPersistentNotifications : Migration
{
    private static readonly string[] UserNotificationIndexColumns =
    [
        "OrganizationId",
        "UserId",
        "ReadAtUtc",
        "CreatedAtUtc"
    ];

    private static readonly string[] SourceMessageUserIndexColumns =
    [
        "SourceMessageId",
        "UserId"
    ];

    protected override void Up(
        MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "notifications",
            columns: table => new
            {
                Id = table.Column<Guid>(
                    type: "uuid",
                    nullable: false),
                OrganizationId = table.Column<Guid>(
                    type: "uuid",
                    nullable: false),
                UserId = table.Column<Guid>(
                    type: "uuid",
                    nullable: false),
                SourceMessageId = table.Column<Guid>(
                    type: "uuid",
                    nullable: false),
                Type = table.Column<string>(
                    type: "character varying(200)",
                    maxLength: 200,
                    nullable: false),
                Title = table.Column<string>(
                    type: "character varying(200)",
                    maxLength: 200,
                    nullable: false),
                Message = table.Column<string>(
                    type: "character varying(1000)",
                    maxLength: 1000,
                    nullable: false),
                RelatedBookingId = table.Column<Guid>(
                    type: "uuid",
                    nullable: true),
                CreatedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false),
                ReadAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey(
                    "PK_notifications",
                    x => x.Id);

                table.ForeignKey(
                    name: "FK_notifications_organizations_OrganizationId",
                    column: x => x.OrganizationId,
                    principalTable: "organizations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);

                table.ForeignKey(
                    name: "FK_notifications_users_UserId",
                    column: x => x.UserId,
                    principalTable: "users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_notifications_OrganizationId_UserId_ReadAtUtc_CreatedAtUtc",
            table: "notifications",
            columns: UserNotificationIndexColumns);

        migrationBuilder.CreateIndex(
            name: "IX_notifications_SourceMessageId_UserId",
            table: "notifications",
            columns: SourceMessageUserIndexColumns,
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_notifications_UserId",
            table: "notifications",
            column: "UserId");
    }

    protected override void Down(
        MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "notifications");
    }
}
