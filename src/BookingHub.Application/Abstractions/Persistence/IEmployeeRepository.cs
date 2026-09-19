using BookingHub.Domain.Employees;

namespace BookingHub.Application.Abstractions.Persistence;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
