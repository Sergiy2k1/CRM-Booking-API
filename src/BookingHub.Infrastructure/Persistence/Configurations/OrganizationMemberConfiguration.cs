using BookingHub.Domain.Organizations;
using BookingHub.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingHub.Infrastructure.Persistence.Configurations;

internal sealed class OrganizationMemberConfiguration
    : IEntityTypeConfiguration<OrganizationMember>
{
    public void Configure(
        EntityTypeBuilder<OrganizationMember> builder)
    {
        builder.ToTable("organization_members");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Role)
            .HasConversion<int>();

        builder.Property(x => x.Status)
            .HasConversion<int>();

        builder.HasIndex(x => new
        {
            x.OrganizationId,
            x.UserId
        })
        .IsUnique();

        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
