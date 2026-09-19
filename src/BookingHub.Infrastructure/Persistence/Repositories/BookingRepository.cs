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

    public Task<Booking?> GetByOrganizationAndIdAsync(
        Guid organizationId,
        Guid bookingId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Bookings
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x =>
                    x.OrganizationId == organizationId &&
                    x.Id == bookingId,
                cancellationToken);
    }

    public Task<Booking?> GetTrackedByOrganizationAndIdAsync(
        Guid organizationId,
        Guid bookingId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Bookings
            .SingleOrDefaultAsync(
                x =>
                    x.OrganizationId == organizationId &&
                    x.Id == bookingId,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<Booking>> ListAsync(
        Guid organizationId,
        BookingStatus? status,
        Guid? employeeId,
        DateTimeOffset? startsFromUtc,
        DateTimeOffset? startsBeforeUtc,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query =
            BuildFilteredQuery(
                organizationId,
                status,
                employeeId,
                startsFromUtc,
                startsBeforeUtc)
            .AsNoTracking();

        return await query
            .OrderBy(x => x.StartsAtUtc)
            .ThenBy(x => x.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(
        Guid organizationId,
        BookingStatus? status,
        Guid? employeeId,
        DateTimeOffset? startsFromUtc,
        DateTimeOffset? startsBeforeUtc,
        CancellationToken cancellationToken = default)
    {
        return BuildFilteredQuery(
                organizationId,
                status,
                employeeId,
                startsFromUtc,
                startsBeforeUtc)
            .CountAsync(cancellationToken);
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

    private IQueryable<Booking> BuildFilteredQuery(
        Guid organizationId,
        BookingStatus? status,
        Guid? employeeId,
        DateTimeOffset? startsFromUtc,
        DateTimeOffset? startsBeforeUtc)
    {
        var query =
            _dbContext.Bookings.Where(
                x => x.OrganizationId == organizationId);

        if (status.HasValue)
        {
            query = query.Where(
                x => x.Status == status.Value);
        }

        if (employeeId.HasValue)
        {
            query = query.Where(
                x => x.EmployeeId == employeeId.Value);
        }

        if (startsFromUtc.HasValue)
        {
            query = query.Where(
                x => x.StartsAtUtc >= startsFromUtc.Value);
        }

        if (startsBeforeUtc.HasValue)
        {
            query = query.Where(
                x => x.StartsAtUtc < startsBeforeUtc.Value);
        }

        return query;
    }
}
