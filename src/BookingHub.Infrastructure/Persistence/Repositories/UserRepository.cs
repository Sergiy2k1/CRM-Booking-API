using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository : IUserRepository
{
    private readonly BookingHubDbContext _dbContext;

    public UserRepository(
        BookingHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetByNormalizedEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.NormalizedEmail == normalizedEmail,
                cancellationToken);
    }
}
