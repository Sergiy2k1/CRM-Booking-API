using BookingHub.Domain.Organizations;
using BookingHub.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingHub.Infrastructure.Persistence.Configurations;

internal sealed class ServiceConfiguration
    : IEntityTypeConfiguration<Service>
{
    public void Configure(
        EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("services");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(Service.MaxNameLength)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(Service.MaxDescriptionLength);

        builder.Property(x => x.PriceAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.Currency)
            .HasMaxLength(Service.CurrencyCodeLength)
            .IsFixedLength()
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>();

        builder.HasIndex(x => new
        {
            x.OrganizationId,
            x.Name
        });

        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
