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

    public Task<bool> HasWorkingHoursOverlapAsync(
        Guid organizationId,
        Guid employeeId,
        DayOfWeek dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.EmployeeWorkingHours
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.OrganizationId == organizationId &&
                    x.EmployeeId == employeeId &&
                    x.DayOfWeek == dayOfWeek &&
                    x.StartTime < endTime &&
                    startTime < x.EndTime,
                cancellationToken);
    }

    public async Task AddWorkingHoursAsync(
        EmployeeWorkingHours workingHours,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.EmployeeWorkingHours.AddAsync(
            workingHours,
            cancellationToken);
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

    public Task<EmployeeTimeOff?> GetTrackedTimeOffByIdAsync(
        Guid organizationId,
        Guid employeeId,
        Guid timeOffId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.EmployeeTimeOffPeriods
            .SingleOrDefaultAsync(
                x =>
                    x.OrganizationId == organizationId &&
                    x.EmployeeId == employeeId &&
                    x.Id == timeOffId,
                cancellationToken);
    }

    public async Task AddTimeOffAsync(
        EmployeeTimeOff timeOff,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.EmployeeTimeOffPeriods.AddAsync(
            timeOff,
            cancellationToken);
    }
}
