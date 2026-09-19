using BookingHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BookingHub.Infrastructure.Persistence.Migrations;

[DbContext(typeof(BookingHubDbContext))]
[Migration("20260920010000_AddWorkingHoursExclusion")]
public sealed class AddWorkingHoursExclusion : Migration
{
    protected override void Up(
        MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            ALTER TABLE employee_working_hours
            ADD CONSTRAINT "EX_working_hours_organization_employee_day_time"
            EXCLUDE USING gist
            (
                "OrganizationId" WITH =,
                "EmployeeId" WITH =,
                "DayOfWeek" WITH =,
                tsrange(
                    DATE '2000-01-01' + "StartTime",
                    DATE '2000-01-01' + "EndTime",
                    '[)'
                ) WITH &&
            );
            """);
    }

    protected override void Down(
        MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            ALTER TABLE employee_working_hours
            DROP CONSTRAINT IF EXISTS "EX_working_hours_organization_employee_day_time";
            """);
    }
}
