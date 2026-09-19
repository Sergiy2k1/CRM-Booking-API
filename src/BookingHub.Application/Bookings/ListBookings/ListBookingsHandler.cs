using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Application.Bookings.Common;

namespace BookingHub.Application.Bookings.ListBookings;

public sealed class ListBookingsHandler
{
    public const int MaxPageSize = 100;

    private readonly IBookingRepository _bookingRepository;

    public ListBookingsHandler(
        IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<ListBookingsResult> HandleAsync(
        ListBookingsQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.Page < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(query),
                "Page must be at least 1.");
        }

        if (query.PageSize < 1 ||
            query.PageSize > MaxPageSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(query),
                $"Page size must be between 1 and {MaxPageSize}.");
        }

        if (query.Status.HasValue &&
            !Enum.IsDefined(query.Status.Value))
        {
            throw new ArgumentOutOfRangeException(
                nameof(query),
                "Booking status is invalid.");
        }

        if (query.StartsFromUtc.HasValue &&
            query.StartsBeforeUtc.HasValue &&
            query.StartsBeforeUtc <= query.StartsFromUtc)
        {
            throw new ArgumentException(
                "Booking list end must be later than start.",
                nameof(query));
        }

        var skip =
            (query.Page - 1) * query.PageSize;

        var bookings =
            await _bookingRepository.ListAsync(
                query.OrganizationId,
                query.Status,
                query.EmployeeId,
                query.StartsFromUtc?.ToUniversalTime(),
                query.StartsBeforeUtc?.ToUniversalTime(),
                skip,
                query.PageSize,
                cancellationToken);

        var totalCount =
            await _bookingRepository.CountAsync(
                query.OrganizationId,
                query.Status,
                query.EmployeeId,
                query.StartsFromUtc?.ToUniversalTime(),
                query.StartsBeforeUtc?.ToUniversalTime(),
                cancellationToken);

        return new ListBookingsResult(
            bookings.Select(x => x.ToDetails()).ToArray(),
            query.Page,
            query.PageSize,
            totalCount);
    }
}
