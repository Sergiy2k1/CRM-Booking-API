namespace BookingHub.Api.Contracts.Reporting;

public sealed record RequestBookingCsvExportRequest(
    DateTimeOffset FromUtc,
    DateTimeOffset ToUtc);
