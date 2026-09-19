using BookingHub.Domain.Bookings;

namespace BookingHub.Application.Reporting.BookingExports;

public sealed record BookingExportRow(
    Guid BookingId,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc,
    BookingStatus Status,
    string CustomerName,
    string EmployeeName,
    string ServiceName,
    decimal PriceAmount,
    string Currency,
    string? Notes);
