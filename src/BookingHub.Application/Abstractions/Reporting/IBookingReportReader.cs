using BookingHub.Application.Reporting.BookingSummary;

namespace BookingHub.Application.Abstractions.Reporting;

public interface IBookingReportReader
{
    Task<BookingSummaryReport> GetSummaryAsync(
        Guid organizationId,
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        CancellationToken cancellationToken = default);
}
