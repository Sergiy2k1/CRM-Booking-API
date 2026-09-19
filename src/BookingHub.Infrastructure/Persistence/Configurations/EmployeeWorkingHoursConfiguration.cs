using BookingHub.Domain.Employees;
using BookingHub.Domain.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingHub.Infrastructure.Persistence.Configurations;

internal sealed class EmployeeWorkingHoursConfiguration
    : IEntityTypeConfiguration<EmployeeWorkingHours>
{
    public void Configure(
        EntityTypeBuilder<EmployeeWorkingHours> builder)
    {
        builder.ToTable("employee_working_hours");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DayOfWeek)
            .HasConversion<int>();

        builder.HasIndex(x => new
        {
            x.OrganizationId,
            x.EmployeeId,
            x.DayOfWeek
        });

        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
