namespace BookingHub.Application.Reporting.BookingExports.DownloadBookingCsvExport;

public sealed record BookingExportFileResult(
    Stream Content,
    string FileName);
