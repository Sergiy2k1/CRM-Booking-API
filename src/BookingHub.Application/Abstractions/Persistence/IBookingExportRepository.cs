using BookingHub.Domain.Reporting;

namespace BookingHub.Application.Abstractions.Persistence;

public interface IBookingExportRepository
{
    Task<BookingExportJob?> GetByOrganizationUserAndIdAsync(
        Guid organizationId,
        Guid userId,
        Guid exportId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        BookingExportJob exportJob,
        CancellationToken cancellationToken = default);
}
