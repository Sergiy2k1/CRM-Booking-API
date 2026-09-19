namespace BookingHub.Api.Contracts.Reporting;

public sealed record BookingSummaryReportResponse(
    Guid OrganizationId,
    DateTimeOffset FromUtc,
    DateTimeOffset ToUtc,
    int TotalBookings,
    int PendingCount,
    int ConfirmedCount,
    int CompletedCount,
    int CancelledCount,
    int NoShowCount,
    IReadOnlyCollection<BookingRevenueResponse> CompletedRevenue);
