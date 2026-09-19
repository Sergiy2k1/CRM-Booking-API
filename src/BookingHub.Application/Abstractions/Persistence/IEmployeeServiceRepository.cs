namespace BookingHub.Application.Abstractions.Persistence;

public interface IEmployeeServiceRepository
{
    Task<bool> IsAssignedAsync(
        Guid organizationId,
        Guid employeeId,
        Guid serviceId,
        CancellationToken cancellationToken = default);
}
