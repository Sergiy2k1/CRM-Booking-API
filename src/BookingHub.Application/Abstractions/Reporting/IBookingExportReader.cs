using BookingHub.Application.Reporting.BookingExports;

namespace BookingHub.Application.Abstractions.Reporting;

public interface IBookingExportReader
{
    Task<IReadOnlyCollection<BookingExportRow>> ReadAsync(
        Guid organizationId,
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        CancellationToken cancellationToken = default);
}
