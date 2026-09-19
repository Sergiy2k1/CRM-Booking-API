using BookingHub.Domain.Bookings;

namespace BookingHub.Api.Contracts.Bookings;

public sealed record BookingResponse(
    Guid Id,
    Guid OrganizationId,
    Guid CustomerId,
    Guid EmployeeId,
    Guid ServiceId,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc,
    decimal PriceAmount,
    string Currency,
    string? Notes,
    BookingStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? CancelledAtUtc,
    DateTimeOffset? CompletedAtUtc,
    DateTimeOffset? NoShowAtUtc);
