using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using BookingHub.Api.Contracts.Reporting;
using BookingHub.Application.Abstractions.Authentication;
using BookingHub.Application.Abstractions.Reporting;
using BookingHub.Application.Reporting.BookingSummary;
using BookingHub.Domain.Organizations;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using Xunit;

namespace BookingHub.Api.IntegrationTests.Reporting;

public sealed class ReportsEndpointTests
{
    private const string SigningKey =
        "development-only-signing-key-change-before-production-2026";

    [Fact]
    public async Task BookingSummaryWithManagerRoleShouldReturnReport()
    {
        var organizationId = Guid.NewGuid();

        var fromUtc =
            new DateTimeOffset(
                2026,
                9,
                1,
                0,
                0,
                0,
                TimeSpan.Zero);

        var toUtc =
            fromUtc.AddDays(30);

        var reader =
            Substitute.For<IBookingReportReader>();

        reader
            .GetSummaryAsync(
                organizationId,
                fromUtc,
                toUtc,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult(
                    new BookingSummaryReport(
                        organizationId,
                        fromUtc,
                        toUtc,
                        3,
                        1,
                        0,
                        1,
                        1,
                        0,
                        [
                            new BookingRevenueByCurrency(
                                "UAH",
                                700m)
                        ])));

        using var application =
            CreateApplication(reader);

        using var client =
            application.CreateClient();

        AddAuthorizationHeader(
            client,
            CreateAccessToken(
                organizationId,
                OrganizationRole.Manager));

        using var response =
            await client.GetAsync(
                CreateSummaryUrl(
                    organizationId,
                    fromUtc,
                    toUtc),
                TestContext.Current.CancellationToken);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var body =
            await response.Content.ReadFromJsonAsync<BookingSummaryReportResponse>(
                cancellationToken:
                    TestContext.Current.CancellationToken);

        Assert.NotNull(body);
        Assert.Equal(3, body.TotalBookings);
        Assert.Single(body.CompletedRevenue);
        Assert.Equal(
            700m,
            body.CompletedRevenue.Single().Amount);
    }

    [Fact]
    public async Task BookingSummaryWithReceptionistRoleShouldReturnForbidden()
    {
        var organizationId = Guid.NewGuid();

        var reader =
            Substitute.For<IBookingReportReader>();

        using var application =
            CreateApplication(reader);

        using var client =
            application.CreateClient();

        AddAuthorizationHeader(
            client,
            CreateAccessToken(
                organizationId,
                OrganizationRole.Receptionist));

        var fromUtc =
            DateTimeOffset.UtcNow;

        using var response =
            await client.GetAsync(
                CreateSummaryUrl(
                    organizationId,
                    fromUtc,
                    fromUtc.AddDays(1)),
                TestContext.Current.CancellationToken);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    private static WebApplicationFactory<Program> CreateApplication(
        IBookingReportReader reader)
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(
                builder =>
                    builder.ConfigureTestServices(
                        services =>
                        {
                            services.RemoveAll<IBookingReportReader>();
                            services.AddSingleton(reader);
                        }));
    }

    private static string CreateSummaryUrl(
        Guid organizationId,
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc)
    {
        var from =
            Uri.EscapeDataString(
                fromUtc.ToString("O"));

        var to =
            Uri.EscapeDataString(
                toUtc.ToString("O"));

        return $"/api/organizations/{organizationId}/reports/bookings/summary?fromUtc={from}&toUtc={to}";
    }

    private static void AddAuthorizationHeader(
        HttpClient client,
        string accessToken)
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);
    }

    private static string CreateAccessToken(
        Guid organizationId,
        OrganizationRole role)
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
                    organizationId.ToString()),
                new Claim(
                    ClaimTypes.Role,
                    role.ToString())
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
