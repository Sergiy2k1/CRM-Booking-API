using BookingHub.Domain.Users;

namespace BookingHub.Application.Abstractions.Persistence;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken = default);
}
