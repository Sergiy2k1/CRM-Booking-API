using BookingHub.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Infrastructure.Persistence.Repositories;

internal sealed class EmployeeServiceRepository
    : IEmployeeServiceRepository
{
    private readonly BookingHubDbContext _dbContext;

    public EmployeeServiceRepository(
        BookingHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> IsAssignedAsync(
        Guid organizationId,
        Guid employeeId,
        Guid serviceId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.EmployeeServices
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.OrganizationId == organizationId &&
                    x.EmployeeId == employeeId &&
                    x.ServiceId == serviceId,
                cancellationToken);
    }
}
