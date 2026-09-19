using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using BookingHub.Api.Contracts.Reporting;
using BookingHub.Application.Abstractions;
using BookingHub.Application.Abstractions.Authentication;
using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Domain.Organizations;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using Xunit;

namespace BookingHub.Api.IntegrationTests.Reporting;

public sealed class BookingCsvExportEndpointTests
{
    private const string SigningKey =
        "development-only-signing-key-change-before-production-2026";

    [Fact]
    public async Task RequestWithManagerRoleShouldReturnAccepted()
    {
        var organizationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var exportId = Guid.NewGuid();

        var repository =
            Substitute.For<IBookingExportRepository>();

        var guidGenerator =
            Substitute.For<IGuidGenerator>();

        var clock =
            Substitute.For<IClock>();

        var unitOfWork =
            Substitute.For<IUnitOfWork>();

        var now =
            new DateTimeOffset(
                2026,
                9,
                20,
                10,
                0,
                0,
                TimeSpan.Zero);

        guidGenerator.NewGuid()
            .Returns(exportId);

        clock.UtcNow.Returns(now);

        repository
            .AddAsync(
                Arg.Any<BookingHub.Domain.Reporting.BookingExportJob>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        unitOfWork
            .SaveChangesAsync(
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        using var application =
            CreateApplication(
                repository,
                guidGenerator,
                clock,
                unitOfWork);

        using var client =
            application.CreateClient();

        AddAuthorizationHeader(
            client,
            CreateAccessToken(
                userId,
                organizationId,
                OrganizationRole.Manager));

        using var response =
            await client.PostAsJsonAsync(
                $"/api/organizations/{organizationId}/reports/bookings/exports",
                new RequestBookingCsvExportRequest(
                    now.AddDays(-30),
                    now),
                TestContext.Current.CancellationToken);

        Assert.Equal(
            HttpStatusCode.Accepted,
            response.StatusCode);

        Assert.NotNull(
            response.Headers.Location);

        Assert.Contains(
            exportId.ToString(),
            response.Headers.Location.ToString(),
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RequestWithReceptionistRoleShouldReturnForbidden()
    {
        var organizationId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var repository =
            Substitute.For<IBookingExportRepository>();

        var guidGenerator =
            Substitute.For<IGuidGenerator>();

        var clock =
            Substitute.For<IClock>();

        var unitOfWork =
            Substitute.For<IUnitOfWork>();

        using var application =
            CreateApplication(
                repository,
                guidGenerator,
                clock,
                unitOfWork);

        using var client =
            application.CreateClient();

        AddAuthorizationHeader(
            client,
            CreateAccessToken(
                userId,
                organizationId,
                OrganizationRole.Receptionist));

        using var response =
            await client.PostAsJsonAsync(
                $"/api/organizations/{organizationId}/reports/bookings/exports",
                new RequestBookingCsvExportRequest(
                    DateTimeOffset.UtcNow.AddDays(-1),
                    DateTimeOffset.UtcNow),
                TestContext.Current.CancellationToken);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    private static WebApplicationFactory<Program> CreateApplication(
        IBookingExportRepository repository,
        IGuidGenerator guidGenerator,
        IClock clock,
        IUnitOfWork unitOfWork)
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(
                builder =>
                    builder.ConfigureTestServices(
                        services =>
                        {
                            services.RemoveAll<IBookingExportRepository>();
                            services.RemoveAll<IGuidGenerator>();
                            services.RemoveAll<IClock>();
                            services.RemoveAll<IUnitOfWork>();

                            services.AddSingleton(repository);
                            services.AddSingleton(guidGenerator);
                            services.AddSingleton(clock);
                            services.AddSingleton(unitOfWork);
                        }));
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
        Guid userId,
        Guid organizationId,
        OrganizationRole role)
    {
        var now = DateTime.UtcNow;

        var claims =
            new[]
            {
                new Claim(
                    JwtRegisteredClaimNames.Sub,
                    userId.ToString()),
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
