using BookingHub.Domain.Notifications;
using BookingHub.Domain.Organizations;
using BookingHub.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingHub.Infrastructure.Persistence.Configurations;

internal sealed class NotificationConfiguration
    : IEntityTypeConfiguration<Notification>
{
    public void Configure(
        EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type)
            .HasMaxLength(Notification.MaxTypeLength)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(Notification.MaxTitleLength)
            .IsRequired();

        builder.Property(x => x.Message)
            .HasMaxLength(Notification.MaxMessageLength)
            .IsRequired();

        builder.Ignore(x => x.IsRead);

        builder.HasIndex(
            x => new
            {
                x.SourceMessageId,
                x.UserId
            })
            .IsUnique();

        builder.HasIndex(
            x => new
            {
                x.OrganizationId,
                x.UserId,
                x.ReadAtUtc,
                x.CreatedAtUtc
            });

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
