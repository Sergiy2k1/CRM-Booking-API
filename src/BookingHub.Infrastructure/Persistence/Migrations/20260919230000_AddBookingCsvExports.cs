using BookingHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BookingHub.Infrastructure.Persistence.Migrations;

[DbContext(typeof(BookingHubDbContext))]
[Migration("20260919230000_AddBookingCsvExports")]
public sealed class AddBookingCsvExports : Migration
{
    private static readonly string[] StatusCreatedIndexColumns =
    [
        "Status",
        "CreatedAtUtc"
    ];

    private static readonly string[] UserCreatedIndexColumns =
    [
        "OrganizationId",
        "RequestedByUserId",
        "CreatedAtUtc"
    ];

    protected override void Up(
        MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "booking_export_jobs",
            columns: table => new
            {
                Id = table.Column<Guid>(
                    type: "uuid",
                    nullable: false),
                OrganizationId = table.Column<Guid>(
                    type: "uuid",
                    nullable: false),
                RequestedByUserId = table.Column<Guid>(
                    type: "uuid",
                    nullable: false),
                FromUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false),
                ToUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false),
                Status = table.Column<int>(
                    type: "integer",
                    nullable: false),
                StorageKey = table.Column<string>(
                    type: "character varying(500)",
                    maxLength: 500,
                    nullable: true),
                FileName = table.Column<string>(
                    type: "character varying(255)",
                    maxLength: 255,
                    nullable: true),
                CreatedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false),
                StartedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: true),
                CompletedAtUtc = table.Column<DateTimeOffset>(
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
                    "PK_booking_export_jobs",
                    x => x.Id);

                table.ForeignKey(
                    name: "FK_booking_export_jobs_organizations_OrganizationId",
                    column: x => x.OrganizationId,
                    principalTable: "organizations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);

                table.ForeignKey(
                    name: "FK_booking_export_jobs_users_RequestedByUserId",
                    column: x => x.RequestedByUserId,
                    principalTable: "users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_booking_export_jobs_OrganizationId_RequestedByUserId_CreatedAtUtc",
            table: "booking_export_jobs",
            columns: UserCreatedIndexColumns);

        migrationBuilder.CreateIndex(
            name: "IX_booking_export_jobs_RequestedByUserId",
            table: "booking_export_jobs",
            column: "RequestedByUserId");

        migrationBuilder.CreateIndex(
            name: "IX_booking_export_jobs_Status_CreatedAtUtc",
            table: "booking_export_jobs",
            columns: StatusCreatedIndexColumns);
    }

    protected override void Down(
        MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "booking_export_jobs");
    }
}
