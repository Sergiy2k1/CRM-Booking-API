using BookingHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BookingHub.Infrastructure.Persistence.Migrations;

[DbContext(typeof(BookingHubDbContext))]
[Migration("20260919193000_AddAuthenticationIdentity")]
public sealed class AddAuthenticationIdentity : Migration
{
    private static readonly string[] OrganizationMemberIdentityColumns =
    [
        "OrganizationId",
        "UserId"
    ];

    protected override void Up(
        MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "users",
            columns: table => new
            {
                Id = table.Column<Guid>(
                    type: "uuid",
                    nullable: false),
                Email = table.Column<string>(
                    type: "character varying(320)",
                    maxLength: 320,
                    nullable: false),
                NormalizedEmail = table.Column<string>(
                    type: "character varying(320)",
                    maxLength: 320,
                    nullable: false),
                PasswordHash = table.Column<string>(
                    type: "character varying(1000)",
                    maxLength: 1000,
                    nullable: false),
                FirstName = table.Column<string>(
                    type: "character varying(100)",
                    maxLength: 100,
                    nullable: false),
                LastName = table.Column<string>(
                    type: "character varying(100)",
                    maxLength: 100,
                    nullable: false),
                IsActive = table.Column<bool>(
                    type: "boolean",
                    nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey(
                    "PK_users",
                    x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "organization_members",
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
                Role = table.Column<int>(
                    type: "integer",
                    nullable: false),
                Status = table.Column<int>(
                    type: "integer",
                    nullable: false),
                JoinedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false),
                RemovedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey(
                    "PK_organization_members",
                    x => x.Id);

                table.ForeignKey(
                    name: "FK_organization_members_organizations_OrganizationId",
                    column: x => x.OrganizationId,
                    principalTable: "organizations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);

                table.ForeignKey(
                    name: "FK_organization_members_users_UserId",
                    column: x => x.UserId,
                    principalTable: "users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_users_NormalizedEmail",
            table: "users",
            column: "NormalizedEmail",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_organization_members_UserId",
            table: "organization_members",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_organization_members_OrganizationId_UserId",
            table: "organization_members",
            columns: OrganizationMemberIdentityColumns,
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_employees_UserId",
            table: "employees",
            column: "UserId");

        migrationBuilder.AddForeignKey(
            name: "FK_employees_users_UserId",
            table: "employees",
            column: "UserId",
            principalTable: "users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(
        MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_employees_users_UserId",
            table: "employees");

        migrationBuilder.DropIndex(
            name: "IX_employees_UserId",
            table: "employees");

        migrationBuilder.DropTable(
            name: "organization_members");

        migrationBuilder.DropTable(
            name: "users");
    }
}
