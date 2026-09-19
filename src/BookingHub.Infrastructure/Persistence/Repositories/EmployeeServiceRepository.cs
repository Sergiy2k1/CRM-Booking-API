using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Domain.Services;
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

    public async Task<IReadOnlyCollection<EmployeeService>> ListByEmployeeAsync(
        Guid organizationId,
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.EmployeeServices
            .AsNoTracking()
            .Where(
                x =>
                    x.OrganizationId == organizationId &&
                    x.EmployeeId == employeeId)
            .ToListAsync(cancellationToken);
    }

    public Task<EmployeeService?> GetTrackedAsync(
        Guid organizationId,
        Guid employeeId,
        Guid serviceId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.EmployeeServices
            .SingleOrDefaultAsync(
                x =>
                    x.OrganizationId == organizationId &&
                    x.EmployeeId == employeeId &&
                    x.ServiceId == serviceId,
                cancellationToken);
    }

    public async Task AddAsync(
        EmployeeService assignment,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.EmployeeServices.AddAsync(
            assignment,
            cancellationToken);
    }

    public void Remove(EmployeeService assignment)
    {
        _dbContext.EmployeeServices.Remove(
            assignment);
    }
}
