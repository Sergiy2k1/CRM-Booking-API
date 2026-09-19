using BookingHub.Domain.Employees;
using BookingHub.Domain.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingHub.Infrastructure.Persistence.Configurations;

internal sealed class EmployeeTimeOffConfiguration
    : IEntityTypeConfiguration<EmployeeTimeOff>
{
    public void Configure(
        EntityTypeBuilder<EmployeeTimeOff> builder)
    {
        builder.ToTable("employee_time_off");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Reason)
            .HasMaxLength(EmployeeTimeOff.MaxReasonLength);

        builder.Property(x => x.Status)
            .HasConversion<int>();

        builder.HasIndex(x => new
        {
            x.OrganizationId,
            x.EmployeeId,
            x.StartsAtUtc,
            x.EndsAtUtc
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
