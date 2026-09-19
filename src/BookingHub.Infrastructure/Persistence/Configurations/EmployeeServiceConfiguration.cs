using BookingHub.Domain.Employees;
using BookingHub.Domain.Organizations;
using BookingHub.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingHub.Infrastructure.Persistence.Configurations;

internal sealed class EmployeeServiceConfiguration
    : IEntityTypeConfiguration<EmployeeService>
{
    public void Configure(
        EntityTypeBuilder<EmployeeService> builder)
    {
        builder.ToTable("employee_services");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new
        {
            x.OrganizationId,
            x.EmployeeId,
            x.ServiceId
        })
        .IsUnique();

        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Service>()
            .WithMany()
            .HasForeignKey(x => x.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
