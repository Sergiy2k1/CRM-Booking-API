using BookingHub.Application.Abstractions.Reporting;

namespace BookingHub.Application.Reporting.BookingSummary;

public sealed class GetBookingSummaryReportHandler
{
    private readonly IBookingReportReader _reportReader;

    public GetBookingSummaryReportHandler(
        IBookingReportReader reportReader)
    {
        _reportReader = reportReader;
    }

    public Task<BookingSummaryReport> HandleAsync(
        GetBookingSummaryReportQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.OrganizationId == Guid.Empty)
        {
            throw new ArgumentException(
                "Organization id cannot be empty.",
                nameof(query));
        }

        var fromUtc =
            query.FromUtc.ToUniversalTime();

        var toUtc =
            query.ToUtc.ToUniversalTime();

        if (toUtc <= fromUtc)
        {
            throw new ArgumentException(
                "Report end must be later than report start.",
                nameof(query));
        }

        return _reportReader.GetSummaryAsync(
            query.OrganizationId,
            fromUtc,
            toUtc,
            cancellationToken);
    }
}
