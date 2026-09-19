using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Application.Abstractions.Reporting;
using BookingHub.Application.Common.Exceptions;
using BookingHub.Domain.Reporting;

namespace BookingHub.Application.Reporting.BookingExports.DownloadBookingCsvExport;

public sealed class DownloadBookingCsvExportHandler
{
    private readonly IBookingExportRepository _exportRepository;
    private readonly IExportFileStorage _fileStorage;

    public DownloadBookingCsvExportHandler(
        IBookingExportRepository exportRepository,
        IExportFileStorage fileStorage)
    {
        _exportRepository = exportRepository;
        _fileStorage = fileStorage;
    }

    public async Task<BookingExportFileResult> HandleAsync(
        DownloadBookingCsvExportQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var exportJob =
            await _exportRepository.GetByOrganizationUserAndIdAsync(
                query.OrganizationId,
                query.UserId,
                query.ExportId,
                cancellationToken);

        if (exportJob is null)
        {
            throw new EntityNotFoundException(
                nameof(BookingExportJob),
                query.ExportId);
        }

        if (exportJob.Status != BookingExportStatus.Completed ||
            string.IsNullOrWhiteSpace(exportJob.StorageKey) ||
            string.IsNullOrWhiteSpace(exportJob.FileName))
        {
            throw new InvalidOperationException(
                "The export file is not ready yet.");
        }

        var stream =
            await _fileStorage.OpenReadAsync(
                exportJob.StorageKey,
                cancellationToken);

        return new BookingExportFileResult(
            stream,
            exportJob.FileName);
    }
}
