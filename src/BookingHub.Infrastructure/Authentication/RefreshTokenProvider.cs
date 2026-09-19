using System.Security.Cryptography;
using System.Text;
using BookingHub.Application.Abstractions.Authentication;
using Microsoft.Extensions.Configuration;

namespace BookingHub.Infrastructure.Authentication;

internal sealed class RefreshTokenProvider
    : IRefreshTokenProvider
{
    private const int RandomTokenByteLength = 64;

    private readonly int _refreshTokenDays;

    public RefreshTokenProvider(
        IConfiguration configuration)
    {
        if (!int.TryParse(
                configuration["Jwt:RefreshTokenDays"],
                out _refreshTokenDays) ||
            _refreshTokenDays <= 0)
        {
            throw new InvalidOperationException(
                "Refresh token lifetime is not configured correctly.");
        }
    }

    public GeneratedRefreshToken Generate(
        DateTimeOffset issuedAtUtc)
    {
        var randomBytes =
            RandomNumberGenerator.GetBytes(
                RandomTokenByteLength);

        var value =
            Convert.ToBase64String(
                randomBytes);

        return new GeneratedRefreshToken(
            value,
            Hash(value),
            issuedAtUtc.AddDays(
                _refreshTokenDays));
    }

    public string Hash(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        var tokenBytes =
            Encoding.UTF8.GetBytes(token);

        var hashBytes =
            SHA256.HashData(tokenBytes);

        return Convert.ToHexString(hashBytes);
    }
}
