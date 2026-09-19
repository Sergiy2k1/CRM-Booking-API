namespace BookingHub.Application.Reporting.BookingExports.GetBookingCsvExport;

public sealed record GetBookingCsvExportQuery(
    Guid OrganizationId,
    Guid UserId,
    Guid ExportId);
