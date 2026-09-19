using BookingHub.Domain.Customers;
using BookingHub.Domain.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingHub.Infrastructure.Persistence.Configurations;

internal sealed class CustomerConfiguration
    : IEntityTypeConfiguration<Customer>
{
    public void Configure(
        EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FirstName)
            .HasMaxLength(Customer.MaxFirstNameLength)
            .IsRequired();

        builder.Property(x => x.LastName)
            .HasMaxLength(Customer.MaxLastNameLength);

        builder.Property(x => x.Email)
            .HasMaxLength(Customer.MaxEmailLength);

        builder.Property(x => x.Phone)
            .HasMaxLength(Customer.MaxPhoneLength);

        builder.Property(x => x.Status)
            .HasConversion<int>();

        builder.HasIndex(x => new
        {
            x.OrganizationId,
            x.LastName,
            x.FirstName
        });

        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
