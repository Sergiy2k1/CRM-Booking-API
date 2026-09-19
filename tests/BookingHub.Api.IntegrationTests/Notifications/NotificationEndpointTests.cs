using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using BookingHub.Api.Contracts.Notifications;
using BookingHub.Application.Abstractions;
using BookingHub.Application.Abstractions.Authentication;
using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Domain.Notifications;
using BookingHub.Domain.Organizations;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using Xunit;

namespace BookingHub.Api.IntegrationTests.Notifications;

public sealed class NotificationEndpointTests
{
    private const string SigningKey =
        "development-only-signing-key-change-before-production-2026";

    [Fact]
    public async Task GetUnreadShouldReturnCurrentUserNotifications()
    {
        var context = CreateContext();
        var dependencies = ConfigureDependencies(context);

        dependencies.NotificationRepository
            .ListAsync(
                context.OrganizationId,
                context.UserId,
                false,
                0,
                20,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<IReadOnlyCollection<Notification>>(
                    [context.Notification]));

        dependencies.NotificationRepository
            .CountAsync(
                context.OrganizationId,
                context.UserId,
                false,
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(1));

        using var application =
            CreateApplication(dependencies);

        using var client =
            application.CreateClient();

        AddAuthorizationHeader(
            client,
            CreateAccessToken(
                context.UserId,
                context.OrganizationId));

        using var response =
            await client.GetAsync(
                $"/api/organizations/{context.OrganizationId}/notifications?isRead=false",
                TestContext.Current.CancellationToken);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var body =
            await response.Content.ReadFromJsonAsync<NotificationListResponse>(
                cancellationToken:
                    TestContext.Current.CancellationToken);

        Assert.NotNull(body);
        Assert.Single(body.Items);
        Assert.Equal(
            context.NotificationId,
            body.Items.Single().Id);
        Assert.False(
            body.Items.Single().IsRead);
    }

    [Fact]
    public async Task MarkReadShouldReturnReadNotification()
    {
        var context = CreateContext();
        var dependencies = ConfigureDependencies(context);

        dependencies.NotificationRepository
            .GetTrackedAsync(
                context.OrganizationId,
                context.UserId,
                context.NotificationId,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<Notification?>(
                    context.Notification));

        using var application =
            CreateApplication(dependencies);

        using var client =
            application.CreateClient();

        AddAuthorizationHeader(
            client,
            CreateAccessToken(
                context.UserId,
                context.OrganizationId));

        using var response =
            await client.PostAsync(
                $"/api/organizations/{context.OrganizationId}/notifications/{context.NotificationId}/read",
                null,
                TestContext.Current.CancellationToken);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var body =
            await response.Content.ReadFromJsonAsync<NotificationResponse>(
                cancellationToken:
                    TestContext.Current.CancellationToken);

        Assert.NotNull(body);
        Assert.True(body.IsRead);
        Assert.Equal(
            context.ReadAtUtc,
            body.ReadAtUtc);
    }

    private static WebApplicationFactory<Program> CreateApplication(
        TestDependencies dependencies)
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(
                builder =>
                    builder.ConfigureTestServices(
                        services =>
                        {
                            Replace(
                                services,
                                dependencies.NotificationRepository);

                            Replace(
                                services,
                                dependencies.Clock);

                            Replace(
                                services,
                                dependencies.UnitOfWork);
                        }));
    }

    private static TestDependencies ConfigureDependencies(
        ApiTestContext context)
    {
        var notificationRepository =
            Substitute.For<INotificationRepository>();

        var clock =
            Substitute.For<IClock>();

        var unitOfWork =
            Substitute.For<IUnitOfWork>();

        clock.UtcNow.Returns(
            context.ReadAtUtc);

        unitOfWork
            .SaveChangesAsync(
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        return new TestDependencies(
            notificationRepository,
            clock,
            unitOfWork);
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
        Guid organizationId)
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

    private static ApiTestContext CreateContext()
    {
        var organizationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();

        var createdAtUtc =
            new DateTimeOffset(
                2026,
                9,
                19,
                21,
                30,
                0,
                TimeSpan.Zero);

        var readAtUtc =
            createdAtUtc.AddMinutes(5);

        var notification =
            Notification.Create(
                notificationId,
                organizationId,
                userId,
                Guid.NewGuid(),
                "booking.created",
                "Booking created",
                "A booking was created.",
                Guid.NewGuid(),
                createdAtUtc);

        return new ApiTestContext(
            organizationId,
            userId,
            notificationId,
            readAtUtc,
            notification);
    }

    private static void Replace<TService>(
        IServiceCollection services,
        TService implementation)
        where TService : class
    {
        services.RemoveAll<TService>();
        services.AddSingleton(implementation);
    }

    private sealed record ApiTestContext(
        Guid OrganizationId,
        Guid UserId,
        Guid NotificationId,
        DateTimeOffset ReadAtUtc,
        Notification Notification);

    private sealed record TestDependencies(
        INotificationRepository NotificationRepository,
        IClock Clock,
        IUnitOfWork UnitOfWork);
}
