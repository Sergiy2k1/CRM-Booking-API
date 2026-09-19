using BookingHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BookingHub.Infrastructure.Persistence.Migrations;

[DbContext(typeof(BookingHubDbContext))]
[Migration("20260919203000_AddRefreshTokens")]
public sealed class AddRefreshTokens : Migration
{
    private static readonly string[] UserOrganizationIndexColumns =
    [
        "UserId",
        "OrganizationId"
    ];

    protected override void Up(
        MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "refresh_tokens",
            columns: table => new
            {
                Id = table.Column<Guid>(
                    type: "uuid",
                    nullable: false),
                UserId = table.Column<Guid>(
                    type: "uuid",
                    nullable: false),
                OrganizationId = table.Column<Guid>(
                    type: "uuid",
                    nullable: false),
                TokenHash = table.Column<string>(
                    type: "character varying(128)",
                    maxLength: 128,
                    nullable: false),
                ExpiresAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false),
                RevokedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: true),
                ReplacedByTokenId = table.Column<Guid>(
                    type: "uuid",
                    nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey(
                    "PK_refresh_tokens",
                    x => x.Id);

                table.ForeignKey(
                    name: "FK_refresh_tokens_organizations_OrganizationId",
                    column: x => x.OrganizationId,
                    principalTable: "organizations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);

                table.ForeignKey(
                    name: "FK_refresh_tokens_users_UserId",
                    column: x => x.UserId,
                    principalTable: "users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_refresh_tokens_OrganizationId",
            table: "refresh_tokens",
            column: "OrganizationId");

        migrationBuilder.CreateIndex(
            name: "IX_refresh_tokens_TokenHash",
            table: "refresh_tokens",
            column: "TokenHash",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_refresh_tokens_UserId_OrganizationId",
            table: "refresh_tokens",
            columns: UserOrganizationIndexColumns);
    }

    protected override void Down(
        MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "refresh_tokens");
    }
}
