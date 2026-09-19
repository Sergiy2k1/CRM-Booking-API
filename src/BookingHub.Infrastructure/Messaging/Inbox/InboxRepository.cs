using BookingHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Infrastructure.Messaging.Inbox;

public sealed class InboxRepository
{
    private readonly BookingHubDbContext _dbContext;

    public InboxRepository(
        BookingHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> IsProcessedAsync(
        string consumer,
        Guid messageId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            consumer);

        if (messageId == Guid.Empty)
        {
            throw new ArgumentException(
                "Inbox message id cannot be empty.",
                nameof(messageId));
        }

        return _dbContext.InboxMessages
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.Consumer == consumer &&
                    x.MessageId == messageId,
                cancellationToken);
    }

    public async Task MarkProcessedAsync(
        string consumer,
        Guid messageId,
        DateTimeOffset processedAtUtc,
        CancellationToken cancellationToken = default)
    {
        var message =
            InboxMessage.Create(
                consumer,
                messageId,
                processedAtUtc);

        await _dbContext.InboxMessages.AddAsync(
            message,
            cancellationToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}
