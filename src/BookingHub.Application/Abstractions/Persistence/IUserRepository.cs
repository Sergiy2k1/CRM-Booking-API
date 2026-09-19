using BookingHub.Domain.Users;

namespace BookingHub.Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task<User?> GetByNormalizedEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default);
}
