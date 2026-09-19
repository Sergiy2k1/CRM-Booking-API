using BookingHub.Application.Abstractions;
using BookingHub.Application.Abstractions.Authentication;
using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Application.Authentication.RefreshSession;
using BookingHub.Domain.Organizations;
using BookingHub.Domain.Users;
using NSubstitute;
using Xunit;

namespace BookingHub.Application.UnitTests.Authentication.RefreshSession;

public sealed class RefreshSessionHandlerTests
{
    [Fact]
    public async Task HandleWithActiveRefreshTokenShouldRotateToken()
    {
        var context = CreateContext();
        var dependencies = ConfigureDependencies(context);
        var handler = CreateHandler(dependencies);

        var result =
            await handler.HandleAsync(
                new RefreshSessionCommand("old-refresh-token"),
                TestContext.Current.CancellationToken);

        Assert.Equal("new-access-token", result.AccessToken);
        Assert.Equal("new-refresh-token", result.RefreshToken);
        Assert.Equal(context.ReplacementTokenId, context.CurrentToken.ReplacedByTokenId);
        Assert.Equal(context.UtcNow, context.CurrentToken.RevokedAtUtc);

        await dependencies.RefreshTokenRepository
            .Received(1)
            .AddAsync(
                Arg.Is<RefreshToken>(
                    token =>
                        token.Id == context.ReplacementTokenId &&
                        token.TokenHash == "new-refresh-token-hash"),
                TestContext.Current.CancellationToken);

        await dependencies.UnitOfWork
            .Received(1)
            .SaveChangesAsync(
                TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task HandleWithRevokedRefreshTokenShouldThrowInvalidRefreshTokenException()
    {
        var context = CreateContext();

        context.CurrentToken.Revoke(
            context.UtcNow.AddMinutes(-1));

        var dependencies = ConfigureDependencies(context);
        var handler = CreateHandler(dependencies);

        await Assert.ThrowsAsync<InvalidRefreshTokenException>(
            () => handler.HandleAsync(
                new RefreshSessionCommand("old-refresh-token"),
                TestContext.Current.CancellationToken));

        await dependencies.RefreshTokenRepository
            .DidNotReceive()
            .AddAsync(
                Arg.Any<RefreshToken>(),
                Arg.Any<CancellationToken>());
    }

    private static TestDependencies ConfigureDependencies(
        RefreshTestContext context)
    {
        var refreshTokenRepository =
            Substitute.For<IRefreshTokenRepository>();
        var userRepository =
            Substitute.For<IUserRepository>();
        var organizationMemberRepository =
            Substitute.For<IOrganizationMemberRepository>();
        var organizationRepository =
            Substitute.For<IOrganizationRepository>();
        var accessTokenProvider =
            Substitute.For<IAccessTokenProvider>();
        var refreshTokenProvider =
            Substitute.For<IRefreshTokenProvider>();
        var guidGenerator =
            Substitute.For<IGuidGenerator>();
        var clock =
            Substitute.For<IClock>();
        var unitOfWork =
            Substitute.For<IUnitOfWork>();

        var user =
            User.Create(
                context.UserId,
                "sergiy@example.com",
                "password-hash",
                "Sergiy",
                "Tester",
                context.UtcNow);

        var organization =
            Organization.Create(
                context.OrganizationId,
                "Beauty Studio",
                "beauty-studio",
                "UTC",
                context.UtcNow);

        var membership =
            OrganizationMember.Create(
                Guid.NewGuid(),
                context.OrganizationId,
                context.UserId,
                OrganizationRole.Admin,
                context.UtcNow);

        refreshTokenProvider
            .Hash("old-refresh-token")
            .Returns("old-refresh-token-hash");

        refreshTokenRepository
            .GetByTokenHashAsync(
                "old-refresh-token-hash",
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<RefreshToken?>(
                    context.CurrentToken));

        userRepository
            .GetByIdAsync(
                context.UserId,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<User?>(user));

        organizationMemberRepository
            .GetByOrganizationAndUserAsync(
                context.OrganizationId,
                context.UserId,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<OrganizationMember?>(membership));

        organizationRepository
            .GetByIdAsync(
                context.OrganizationId,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<Organization?>(organization));

        accessTokenProvider
            .Create(
                context.UserId,
                context.OrganizationId,
                "sergiy@example.com",
                OrganizationRole.Admin,
                context.UtcNow)
            .Returns(
                new AccessToken(
                    "new-access-token",
                    context.UtcNow.AddMinutes(15)));

        refreshTokenProvider
            .Generate(context.UtcNow)
            .Returns(
                new GeneratedRefreshToken(
                    "new-refresh-token",
                    "new-refresh-token-hash",
                    context.UtcNow.AddDays(30)));

        guidGenerator.NewGuid()
            .Returns(context.ReplacementTokenId);

        clock.UtcNow.Returns(context.UtcNow);

        refreshTokenRepository
            .AddAsync(
                Arg.Any<RefreshToken>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        unitOfWork
            .SaveChangesAsync(
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        return new TestDependencies(
            refreshTokenRepository,
            userRepository,
            organizationMemberRepository,
            organizationRepository,
            accessTokenProvider,
            refreshTokenProvider,
            guidGenerator,
            clock,
            unitOfWork);
    }

    private static RefreshSessionHandler CreateHandler(
        TestDependencies dependencies)
    {
        return new RefreshSessionHandler(
            dependencies.RefreshTokenRepository,
            dependencies.UserRepository,
            dependencies.OrganizationMemberRepository,
            dependencies.OrganizationRepository,
            dependencies.AccessTokenProvider,
            dependencies.RefreshTokenProvider,
            dependencies.GuidGenerator,
            dependencies.Clock,
            dependencies.UnitOfWork);
    }

    private static RefreshTestContext CreateContext()
    {
        var userId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();

        var utcNow =
            new DateTimeOffset(
                2026,
                9,
                19,
                16,
                0,
                0,
                TimeSpan.Zero);

        var currentToken =
            RefreshToken.Create(
                Guid.NewGuid(),
                userId,
                organizationId,
                "old-refresh-token-hash",
                utcNow.AddDays(30),
                utcNow.AddDays(-1));

        return new RefreshTestContext(
            userId,
            organizationId,
            Guid.NewGuid(),
            utcNow,
            currentToken);
    }

    private sealed record RefreshTestContext(
        Guid UserId,
        Guid OrganizationId,
        Guid ReplacementTokenId,
        DateTimeOffset UtcNow,
        RefreshToken CurrentToken);

    private sealed record TestDependencies(
        IRefreshTokenRepository RefreshTokenRepository,
        IUserRepository UserRepository,
        IOrganizationMemberRepository OrganizationMemberRepository,
        IOrganizationRepository OrganizationRepository,
        IAccessTokenProvider AccessTokenProvider,
        IRefreshTokenProvider RefreshTokenProvider,
        IGuidGenerator GuidGenerator,
        IClock Clock,
        IUnitOfWork UnitOfWork);
}
