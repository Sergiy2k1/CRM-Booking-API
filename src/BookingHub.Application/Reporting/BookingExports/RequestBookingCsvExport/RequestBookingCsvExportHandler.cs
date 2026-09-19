using BookingHub.Application.Abstractions;
using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Domain.Reporting;

namespace BookingHub.Application.Reporting.BookingExports.RequestBookingCsvExport;

public sealed class RequestBookingCsvExportHandler
{
    private readonly IBookingExportRepository _exportRepository;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;

    public RequestBookingCsvExportHandler(
        IBookingExportRepository exportRepository,
        IGuidGenerator guidGenerator,
        IClock clock,
        IUnitOfWork unitOfWork)
    {
        _exportRepository = exportRepository;
        _guidGenerator = guidGenerator;
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    public async Task<BookingExportDetails> HandleAsync(
        RequestBookingCsvExportCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var exportJob =
            BookingExportJob.Create(
                _guidGenerator.NewGuid(),
                command.OrganizationId,
                command.RequestedByUserId,
                command.FromUtc,
                command.ToUtc,
                _clock.UtcNow);

        await _exportRepository.AddAsync(
            exportJob,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return exportJob.ToDetails();
    }
}
