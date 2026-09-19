using BookingHub.Domain.Reporting;

namespace BookingHub.Application.Reporting.BookingExports;

internal static class BookingExportMappings
{
    public static BookingExportDetails ToDetails(
        this BookingExportJob exportJob)
    {
        return new BookingExportDetails(
            exportJob.Id,
            exportJob.OrganizationId,
            exportJob.RequestedByUserId,
            exportJob.FromUtc,
            exportJob.ToUtc,
            exportJob.Status,
            exportJob.CreatedAtUtc,
            exportJob.StartedAtUtc,
            exportJob.CompletedAtUtc,
            exportJob.AttemptCount,
            exportJob.LastError,
            exportJob.FileName);
    }
}
