using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Infrastructure.Persistence.Repositories;

internal sealed class RefreshTokenRepository
    : IRefreshTokenRepository
{
    private readonly BookingHubDbContext _dbContext;

    public RefreshTokenRepository(
        BookingHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<RefreshToken?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.RefreshTokens
            .SingleOrDefaultAsync(
                x => x.TokenHash == tokenHash,
                cancellationToken);
    }

    public async Task AddAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.RefreshTokens.AddAsync(
            refreshToken,
            cancellationToken);
    }
}
