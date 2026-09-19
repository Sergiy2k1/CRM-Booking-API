namespace BookingHub.Application.Reporting.BookingExports.RequestBookingCsvExport;

public sealed record RequestBookingCsvExportCommand(
    Guid OrganizationId,
    Guid RequestedByUserId,
    DateTimeOffset FromUtc,
    DateTimeOffset ToUtc);
