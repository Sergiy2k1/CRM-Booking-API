using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using BookingHub.Application.Abstractions.Authentication;
using BookingHub.Domain.Organizations;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace BookingHub.Api.IntegrationTests.Realtime;

public sealed class BookingsHubEndpointTests
{
    private const string SigningKey =
        "development-only-signing-key-change-before-production-2026";

    [Fact]
    public async Task NegotiateWithoutAccessTokenShouldReturnUnauthorized()
    {
        using var application =
            new WebApplicationFactory<Program>();

        using var client =
            application.CreateClient();

        using var response =
            await client.PostAsync(
                "/hubs/bookings/negotiate?negotiateVersion=1",
                null,
                TestContext.Current.CancellationToken);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task NegotiateWithAccessTokenShouldReturnOk()
    {
        using var application =
            new WebApplicationFactory<Program>();

        using var client =
            application.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                CreateAccessToken());

        using var response =
            await client.PostAsync(
                "/hubs/bookings/negotiate?negotiateVersion=1",
                null,
                TestContext.Current.CancellationToken);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }

    private static string CreateAccessToken()
    {
        var now =
            DateTime.UtcNow;

        var claims =
            new[]
            {
                new Claim(
                    JwtRegisteredClaimNames.Sub,
                    Guid.NewGuid().ToString()),
                new Claim(
                    AuthenticationClaimTypes.OrganizationId,
                    Guid.NewGuid().ToString()),
                new Claim(
                    ClaimTypes.Role,
                    OrganizationRole.Receptionist.ToString())
            };

        var credentials =
            new SigningCredentials(
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        SigningKey)),
                SecurityAlgorithms.HmacSha256);

        var token =
            new JwtSecurityToken(
                issuer: "BookingHub",
                audience: "BookingHub.Api",
                claims: claims,
                notBefore: now.AddMinutes(-1),
                expires: now.AddMinutes(15),
                signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}
