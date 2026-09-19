using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Domain.Employees;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Infrastructure.Persistence.Repositories;

internal sealed class EmployeeRepository
    : IEmployeeRepository
{
    private readonly BookingHubDbContext _dbContext;

    public EmployeeRepository(
        BookingHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Employee?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Employees
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public Task<Employee?> GetByOrganizationAndIdAsync(
        Guid organizationId,
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Employees
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x =>
                    x.OrganizationId == organizationId &&
                    x.Id == employeeId,
                cancellationToken);
    }

    public Task<Employee?> GetTrackedByOrganizationAndIdAsync(
        Guid organizationId,
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Employees
            .SingleOrDefaultAsync(
                x =>
                    x.OrganizationId == organizationId &&
                    x.Id == employeeId,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<Employee>> ListAsync(
        Guid organizationId,
        EmployeeStatus? status,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query =
            _dbContext.Employees
                .AsNoTracking()
                .Where(
                    x => x.OrganizationId == organizationId);

        if (status.HasValue)
        {
            query =
                query.Where(
                    x => x.Status == status.Value);
        }

        return await query
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .ThenBy(x => x.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(
        Guid organizationId,
        EmployeeStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query =
            _dbContext.Employees
                .AsNoTracking()
                .Where(
                    x => x.OrganizationId == organizationId);

        if (status.HasValue)
        {
            query =
                query.Where(
                    x => x.Status == status.Value);
        }

        return query.CountAsync(
            cancellationToken);
    }

    public async Task AddAsync(
        Employee employee,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Employees.AddAsync(
            employee,
            cancellationToken);
    }
}
