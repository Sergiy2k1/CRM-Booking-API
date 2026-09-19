namespace BookingHub.Application.Reporting.BookingSummary;

public sealed record BookingSummaryReport(
    Guid OrganizationId,
    DateTimeOffset FromUtc,
    DateTimeOffset ToUtc,
    int TotalBookings,
    int PendingCount,
    int ConfirmedCount,
    int CompletedCount,
    int CancelledCount,
    int NoShowCount,
    IReadOnlyCollection<BookingRevenueByCurrency> CompletedRevenue);
