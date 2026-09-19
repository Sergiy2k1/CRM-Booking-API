using BookingHub.Domain.Bookings;

namespace BookingHub.Application.Abstractions.Persistence;

public interface IBookingRepository
{
    Task<IReadOnlyCollection<Booking>> GetOverlappingAsync(
        Guid organizationId,
        Guid employeeId,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Booking booking,
        CancellationToken cancellationToken = default);
}
