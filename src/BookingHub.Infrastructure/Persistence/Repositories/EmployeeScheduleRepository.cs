using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Domain.Employees;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Infrastructure.Persistence.Repositories;

internal sealed class EmployeeScheduleRepository
    : IEmployeeScheduleRepository
{
    private readonly BookingHubDbContext _dbContext;

    public EmployeeScheduleRepository(
        BookingHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<EmployeeWorkingHours>> GetWorkingHoursAsync(
        Guid organizationId,
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.EmployeeWorkingHours
            .AsNoTracking()
            .Where(
                x =>
                    x.OrganizationId == organizationId &&
                    x.EmployeeId == employeeId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<EmployeeTimeOff>> GetTimeOffAsync(
        Guid organizationId,
        Guid employeeId,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.EmployeeTimeOffPeriods
            .AsNoTracking()
            .Where(
                x =>
                    x.OrganizationId == organizationId &&
                    x.EmployeeId == employeeId &&
                    x.StartsAtUtc < endsAtUtc &&
                    startsAtUtc < x.EndsAtUtc)
            .ToListAsync(cancellationToken);
    }
}
