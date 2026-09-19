using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Application.Common.Exceptions;
using BookingHub.Domain.Reporting;

namespace BookingHub.Application.Reporting.BookingExports.GetBookingCsvExport;

public sealed class GetBookingCsvExportHandler
{
    private readonly IBookingExportRepository _exportRepository;

    public GetBookingCsvExportHandler(
        IBookingExportRepository exportRepository)
    {
        _exportRepository = exportRepository;
    }

    public async Task<BookingExportDetails> HandleAsync(
        GetBookingCsvExportQuery query,
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

        return exportJob.ToDetails();
    }
}
