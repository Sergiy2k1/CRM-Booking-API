using BookingHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BookingHub.Infrastructure.Persistence.Migrations;

[DbContext(typeof(BookingHubDbContext))]
[Migration("20260919183000_InitialBookingPersistence")]
public sealed class InitialBookingPersistence : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            "CREATE EXTENSION IF NOT EXISTS btree_gist;");

        migrationBuilder.CreateTable(
            name: "organizations",
            columns: table => new
            {
                Id = table.Column<Guid>(
                    type: "uuid",
                    nullable: false),
                Name = table.Column<string>(
                    type: "character varying(200)",
                    maxLength: 200,
                    nullable: false),
                Slug = table.Column<string>(
                    type: "character varying(100)",
                    maxLength: 100,
                    nullable: false),
                TimeZone = table.Column<string>(
                    type: "character varying(100)",
                    maxLength: 100,
                    nullable: false),
                Status = table.Column<int>(
                    type: "integer",
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
                    "PK_organizations",
                    x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "customers",
            columns: table => new
            {
                Id = table.Column<Guid>(
                    type: "uuid",
                    nullable: false),
                OrganizationId = table.Column<Guid>(
                    type: "uuid",
                    nullable: false),
                FirstName = table.Column<string>(
                    type: "character varying(100)",
                    maxLength: 100,
                    nullable: false),
                LastName = table.Column<string>(
                    type: "character varying(100)",
                    maxLength: 100,
                    nullable: true),
                Email = table.Column<string>(
                    type: "character varying(320)",
                    maxLength: 320,
                    nullable: true),
                Phone = table.Column<string>(
                    type: "character varying(32)",
                    maxLength: 32,
                    nullable: true),
                Status = table.Column<int>(
                    type: "integer",
                    nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false),
                ArchivedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey(
                    "PK_customers",
                    x => x.Id);

                table.ForeignKey(
                    name: "FK_customers_organizations_OrganizationId",
                    column: x => x.OrganizationId,
                    principalTable: "organizations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "employees",
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
                    nullable: true),
                FirstName = table.Column<string>(
                    type: "character varying(100)",
                    maxLength: 100,
                    nullable: false),
                LastName = table.Column<string>(
                    type: "character varying(100)",
                    maxLength: 100,
                    nullable: true),
                Position = table.Column<string>(
                    type: "character varying(150)",
                    maxLength: 150,
                    nullable: true),
                Status = table.Column<int>(
                    type: "integer",
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
                    "PK_employees",
                    x => x.Id);

                table.ForeignKey(
                    name: "FK_employees_organizations_OrganizationId",
                    column: x => x.OrganizationId,
                    principalTable: "organizations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "services",
            columns: table => new
            {
                Id = table.Column<Guid>(
                    type: "uuid",
                    nullable: false),
                OrganizationId = table.Column<Guid>(
                    type: "uuid",
                    nullable: false),
                Name = table.Column<string>(
                    type: "character varying(200)",
                    maxLength: 200,
                    nullable: false),
                Description = table.Column<string>(
                    type: "character varying(2000)",
                    maxLength: 2000,
                    nullable: true),
                Duration = table.Column<TimeSpan>(
                    type: "interval",
                    nullable: false),
                PriceAmount = table.Column<decimal>(
                    type: "numeric(18,2)",
                    precision: 18,
                    scale: 2,
                    nullable: false),
                Currency = table.Column<string>(
                    type: "character(3)",
                    fixedLength: true,
                    maxLength: 3,
                    nullable: false),
                Status = table.Column<int>(
                    type: "integer",
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
                    "PK_services",
                    x => x.Id);

                table.ForeignKey(
                    name: "FK_services_organizations_OrganizationId",
                    column: x => x.OrganizationId,
                    principalTable: "organizations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "employee_services",
            columns: table => new
            {
                Id = table.Column<Guid>(
                    type: "uuid",
                    nullable: false),
                OrganizationId = table.Column<Guid>(
                    type: "uuid",
                    nullable: false),
                EmployeeId = table.Column<Guid>(
                    type: "uuid",
                    nullable: false),
                ServiceId = table.Column<Guid>(
                    type: "uuid",
                    nullable: false),
                AssignedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey(
                    "PK_employee_services",
                    x => x.Id);

                table.ForeignKey(
                    name: "FK_employee_services_employees_EmployeeId",
                    column: x => x.EmployeeId,
                    principalTable: "employees",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);

                table.ForeignKey(
                    name: "FK_employee_services_organizations_OrganizationId",
                    column: x => x.OrganizationId,
                    principalTable: "organizations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);

                table.ForeignKey(
                    name: "FK_employee_services_services_ServiceId",
                    column: x => x.ServiceId,
                    principalTable: "services",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "employee_working_hours",
            columns: table => new
            {
                Id = table.Column<Guid>(
                    type: "uuid",
                    nullable: false),
                OrganizationId = table.Column<Guid>(
                    type: "uuid",
                    nullable: false),
                EmployeeId = table.Column<Guid>(
                    type: "uuid",
                    nullable: false),
                DayOfWeek = table.Column<int>(
                    type: "integer",
                    nullable: false),
                StartTime = table.Column<TimeOnly>(
                    type: "time without time zone",
                    nullable: false),
                EndTime = table.Column<TimeOnly>(
                    type: "time without time zone",
                    nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey(
                    "PK_employee_working_hours",
                    x => x.Id);

                table.ForeignKey(
                    name: "FK_employee_working_hours_employees_EmployeeId",
                    column: x => x.EmployeeId,
                    principalTable: "employees",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);

                table.ForeignKey(
                    name: "FK_employee_working_hours_organizations_OrganizationId",
                    column: x => x.OrganizationId,
                    principalTable: "organizations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "employee_time_off",
            columns: table => new
            {
                Id = table.Column<Guid>(
                    type: "uuid",
                    nullable: false),
                OrganizationId = table.Column<Guid>(
                    type: "uuid",
                    nullable: false),
                EmployeeId = table.Column<Guid>(
                    type: "uuid",
                    nullable: false),
                StartsAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false),
                EndsAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false),
                Reason = table.Column<string>(
                    type: "character varying(500)",
                    maxLength: 500,
                    nullable: true),
                Status = table.Column<int>(
                    type: "integer",
                    nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false),
                CancelledAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey(
                    "PK_employee_time_off",
                    x => x.Id);

                table.ForeignKey(
                    name: "FK_employee_time_off_employees_EmployeeId",
                    column: x => x.EmployeeId,
                    principalTable: "employees",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);

                table.ForeignKey(
                    name: "FK_employee_time_off_organizations_OrganizationId",
                    column: x => x.OrganizationId,
                    principalTable: "organizations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "bookings",
            columns: table => new
            {
                Id = table.Column<Guid>(
                    type: "uuid",
                    nullable: false),
                OrganizationId = table.Column<Guid>(
                    type: "uuid",
                    nullable: false),
                CustomerId = table.Column<Guid>(
                    type: "uuid",
                    nullable: false),
                EmployeeId = table.Column<Guid>(
                    type: "uuid",
                    nullable: false),
                ServiceId = table.Column<Guid>(
                    type: "uuid",
                    nullable: false),
                StartsAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false),
                EndsAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false),
                PriceAmount = table.Column<decimal>(
                    type: "numeric(18,2)",
                    precision: 18,
                    scale: 2,
                    nullable: false),
                Currency = table.Column<string>(
                    type: "character(3)",
                    fixedLength: true,
                    maxLength: 3,
                    nullable: false),
                Notes = table.Column<string>(
                    type: "character varying(2000)",
                    maxLength: 2000,
                    nullable: true),
                Status = table.Column<int>(
                    type: "integer",
                    nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false),
                CancelledAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: true),
                CompletedAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: true),
                NoShowAtUtc = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey(
                    "PK_bookings",
                    x => x.Id);

                table.ForeignKey(
                    name: "FK_bookings_customers_CustomerId",
                    column: x => x.CustomerId,
                    principalTable: "customers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);

                table.ForeignKey(
                    name: "FK_bookings_employees_EmployeeId",
                    column: x => x.EmployeeId,
                    principalTable: "employees",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);

                table.ForeignKey(
                    name: "FK_bookings_organizations_OrganizationId",
                    column: x => x.OrganizationId,
                    principalTable: "organizations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);

                table.ForeignKey(
                    name: "FK_bookings_services_ServiceId",
                    column: x => x.ServiceId,
                    principalTable: "services",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_organizations_Slug",
            table: "organizations",
            column: "Slug",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_customers_OrganizationId_LastName_FirstName",
            table: "customers",
            columns: new[]
            {
                "OrganizationId",
                "LastName",
                "FirstName"
            });

        migrationBuilder.CreateIndex(
            name: "IX_employees_OrganizationId_FirstName_LastName",
            table: "employees",
            columns: new[]
            {
                "OrganizationId",
                "FirstName",
                "LastName"
            });

        migrationBuilder.CreateIndex(
            name: "IX_services_OrganizationId_Name",
            table: "services",
            columns: new[]
            {
                "OrganizationId",
                "Name"
            });

        migrationBuilder.CreateIndex(
            name: "IX_employee_services_EmployeeId",
            table: "employee_services",
            column: "EmployeeId");

        migrationBuilder.CreateIndex(
            name: "IX_employee_services_ServiceId",
            table: "employee_services",
            column: "ServiceId");

        migrationBuilder.CreateIndex(
            name: "IX_employee_services_OrganizationId_EmployeeId_ServiceId",
            table: "employee_services",
            columns: new[]
            {
                "OrganizationId",
                "EmployeeId",
                "ServiceId"
            },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_employee_working_hours_EmployeeId",
            table: "employee_working_hours",
            column: "EmployeeId");

        migrationBuilder.CreateIndex(
            name: "IX_employee_working_hours_OrganizationId_EmployeeId_DayOfWeek",
            table: "employee_working_hours",
            columns: new[]
            {
                "OrganizationId",
                "EmployeeId",
                "DayOfWeek"
            });

        migrationBuilder.CreateIndex(
            name: "IX_employee_time_off_EmployeeId",
            table: "employee_time_off",
            column: "EmployeeId");

        migrationBuilder.CreateIndex(
            name: "IX_employee_time_off_OrganizationId_EmployeeId_StartsAtUtc_EndsAtUtc",
            table: "employee_time_off",
            columns: new[]
            {
                "OrganizationId",
                "EmployeeId",
                "StartsAtUtc",
                "EndsAtUtc"
            });

        migrationBuilder.CreateIndex(
            name: "IX_bookings_CustomerId",
            table: "bookings",
            column: "CustomerId");

        migrationBuilder.CreateIndex(
            name: "IX_bookings_EmployeeId",
            table: "bookings",
            column: "EmployeeId");

        migrationBuilder.CreateIndex(
            name: "IX_bookings_ServiceId",
            table: "bookings",
            column: "ServiceId");

        migrationBuilder.CreateIndex(
            name: "IX_bookings_OrganizationId_EmployeeId_StartsAtUtc_EndsAtUtc",
            table: "bookings",
            columns: new[]
            {
                "OrganizationId",
                "EmployeeId",
                "StartsAtUtc",
                "EndsAtUtc"
            });

        migrationBuilder.Sql(
            """
            ALTER TABLE bookings
            ADD CONSTRAINT "EX_bookings_organization_employee_time"
            EXCLUDE USING gist
            (
                "OrganizationId" WITH =,
                "EmployeeId" WITH =,
                tstzrange("StartsAtUtc", "EndsAtUtc", '[)') WITH &&
            )
            WHERE ("Status" IN (1, 2));
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "bookings");

        migrationBuilder.DropTable(
            name: "employee_services");

        migrationBuilder.DropTable(
            name: "employee_time_off");

        migrationBuilder.DropTable(
            name: "employee_working_hours");

        migrationBuilder.DropTable(
            name: "customers");

        migrationBuilder.DropTable(
            name: "services");

        migrationBuilder.DropTable(
            name: "employees");

        migrationBuilder.DropTable(
            name: "organizations");
    }
}
