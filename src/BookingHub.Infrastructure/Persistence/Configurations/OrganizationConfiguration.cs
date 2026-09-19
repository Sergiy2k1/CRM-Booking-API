using BookingHub.Domain.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingHub.Infrastructure.Persistence.Configurations;

internal sealed class OrganizationConfiguration
    : IEntityTypeConfiguration<Organization>
{
    public void Configure(
        EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("organizations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(Organization.MaxNameLength)
            .IsRequired();

        builder.Property(x => x.Slug)
            .HasMaxLength(Organization.MaxSlugLength)
            .IsRequired();

        builder.Property(x => x.TimeZone)
            .HasMaxLength(Organization.MaxTimeZoneLength)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>();

        builder.HasIndex(x => x.Slug)
            .IsUnique();
    }
}
