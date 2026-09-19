namespace BookingHub.Application.Reporting.BookingSummary;

public sealed record GetBookingSummaryReportQuery(
    Guid OrganizationId,
    DateTimeOffset FromUtc,
    DateTimeOffset ToUtc);
