namespace BookingHub.Application.Reporting.BookingExports.DownloadBookingCsvExport;

public sealed record DownloadBookingCsvExportQuery(
    Guid OrganizationId,
    Guid UserId,
    Guid ExportId);
