using BookingHub.Infrastructure.Messaging.Inbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingHub.Infrastructure.Persistence.Configurations;

internal sealed class InboxMessageConfiguration
    : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(
        EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("inbox_messages");

        builder.HasKey(
            x => new
            {
                x.Consumer,
                x.MessageId
            });

        builder.Property(x => x.Consumer)
            .HasMaxLength(InboxMessage.MaxConsumerLength)
            .IsRequired();

        builder.HasIndex(x => x.ProcessedAtUtc);
    }
}
