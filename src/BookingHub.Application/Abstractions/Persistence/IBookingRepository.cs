using BookingHub.Domain.Bookings;

namespace BookingHub.Application.Abstractions.Persistence;

public interface IBookingRepository
{
    Task<Booking?> GetByOrganizationAndIdAsync(
        Guid organizationId,
        Guid bookingId,
        CancellationToken cancellationToken = default);

    Task<Booking?> GetTrackedByOrganizationAndIdAsync(
        Guid organizationId,
        Guid bookingId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Booking>> ListAsync(
        Guid organizationId,
        BookingStatus? status,
        Guid? employeeId,
        DateTimeOffset? startsFromUtc,
        DateTimeOffset? startsBeforeUtc,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        Guid organizationId,
        BookingStatus? status,
        Guid? employeeId,
        DateTimeOffset? startsFromUtc,
        DateTimeOffset? startsBeforeUtc,
        CancellationToken cancellationToken = default);

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
