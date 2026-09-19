using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BookingHub.Application.Abstractions.Authentication;
using BookingHub.Domain.Organizations;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BookingHub.Infrastructure.Authentication;

internal sealed class JwtAccessTokenProvider : IAccessTokenProvider
{
    private const int MinimumSigningKeyLength = 32;

    private readonly string _issuer;
    private readonly string _audience;
    private readonly string _signingKey;
    private readonly int _accessTokenMinutes;

    public JwtAccessTokenProvider(
        IConfiguration configuration)
    {
        _issuer =
            configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException(
                "JWT issuer is not configured.");

        _audience =
            configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException(
                "JWT audience is not configured.");

        _signingKey =
            configuration["Jwt:SigningKey"]
            ?? throw new InvalidOperationException(
                "JWT signing key is not configured.");

        if (_signingKey.Length < MinimumSigningKeyLength)
        {
            throw new InvalidOperationException(
                $"JWT signing key must contain at least {MinimumSigningKeyLength} characters.");
        }

        if (!int.TryParse(
                configuration["Jwt:AccessTokenMinutes"],
                out _accessTokenMinutes) ||
            _accessTokenMinutes <= 0)
        {
            throw new InvalidOperationException(
                "JWT access token lifetime is not configured correctly.");
        }
    }

    public AccessToken Create(
        Guid userId,
        Guid organizationId,
        string email,
        OrganizationRole role,
        DateTimeOffset issuedAtUtc)
    {
        var expiresAtUtc =
            issuedAtUtc.AddMinutes(
                _accessTokenMinutes);

        var claims = new[]
        {
            new Claim(
                JwtRegisteredClaimNames.Sub,
                userId.ToString()),
            new Claim(
                JwtRegisteredClaimNames.Email,
                email),
            new Claim(
                "organization_id",
                organizationId.ToString()),
            new Claim(
                ClaimTypes.Role,
                role.ToString()),
            new Claim(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString())
        };

        var signingCredentials =
            new SigningCredentials(
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        _signingKey)),
                SecurityAlgorithms.HmacSha256);

        var token =
            new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                notBefore: issuedAtUtc.UtcDateTime,
                expires: expiresAtUtc.UtcDateTime,
                signingCredentials: signingCredentials);

        return new AccessToken(
            new JwtSecurityTokenHandler()
                .WriteToken(token),
            expiresAtUtc);
    }
}
