using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Domain.Reporting;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Infrastructure.Persistence.Repositories;

internal sealed class BookingExportRepository
    : IBookingExportRepository
{
    private readonly BookingHubDbContext _dbContext;

    public BookingExportRepository(
        BookingHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<BookingExportJob?> GetByOrganizationUserAndIdAsync(
        Guid organizationId,
        Guid userId,
        Guid exportId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.BookingExportJobs
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x =>
                    x.OrganizationId == organizationId &&
                    x.RequestedByUserId == userId &&
                    x.Id == exportId,
                cancellationToken);
    }

    public async Task AddAsync(
        BookingExportJob exportJob,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.BookingExportJobs.AddAsync(
            exportJob,
            cancellationToken);
    }
}
