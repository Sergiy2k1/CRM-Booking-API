using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Domain.Bookings;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Infrastructure.Persistence.Repositories;

internal sealed class BookingRepository
    : IBookingRepository
{
    private readonly BookingHubDbContext _dbContext;

    public BookingRepository(
        BookingHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<Booking>> GetOverlappingAsync(
        Guid organizationId,
        Guid employeeId,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Bookings
            .AsNoTracking()
            .Where(
                x =>
                    x.OrganizationId == organizationId &&
                    x.EmployeeId == employeeId &&
                    (x.Status == BookingStatus.Pending ||
                     x.Status == BookingStatus.Confirmed) &&
                    x.StartsAtUtc < endsAtUtc &&
                    startsAtUtc < x.EndsAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        Booking booking,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Bookings.AddAsync(
            booking,
            cancellationToken);
    }
}
