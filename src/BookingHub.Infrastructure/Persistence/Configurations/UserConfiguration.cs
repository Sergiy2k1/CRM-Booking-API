using BookingHub.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingHub.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration
    : IEntityTypeConfiguration<User>
{
    public void Configure(
        EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
            .HasMaxLength(User.MaxEmailLength)
            .IsRequired();

        builder.Property(x => x.NormalizedEmail)
            .HasMaxLength(User.MaxEmailLength)
            .IsRequired();

        builder.Property(x => x.PasswordHash)
            .HasMaxLength(User.MaxPasswordHashLength)
            .IsRequired();

        builder.Property(x => x.FirstName)
            .HasMaxLength(User.MaxNameLength)
            .IsRequired();

        builder.Property(x => x.LastName)
            .HasMaxLength(User.MaxNameLength)
            .IsRequired();

        builder.HasIndex(x => x.NormalizedEmail)
            .IsUnique();
    }
}
