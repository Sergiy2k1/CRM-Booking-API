using BookingHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Infrastructure.Messaging.Outbox;

public sealed class OutboxRepository
{
    private readonly BookingHubDbContext _dbContext;

    public OutboxRepository(
        BookingHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<OutboxMessage>> GetPendingAsync(
        int batchSize,
        int maxAttempts,
        CancellationToken cancellationToken = default)
    {
        if (batchSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(batchSize));
        }

        if (maxAttempts <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxAttempts));
        }

        return await _dbContext.OutboxMessages
            .Where(
                x =>
                    x.ProcessedAtUtc == null &&
                    x.AttemptCount < maxAttempts)
            .OrderBy(x => x.OccurredAtUtc)
            .ThenBy(x => x.Id)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}
