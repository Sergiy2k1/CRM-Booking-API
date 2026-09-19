using BookingHub.Domain.Services;

namespace BookingHub.Application.Abstractions.Persistence;

public interface IServiceRepository
{
    Task<Service?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
