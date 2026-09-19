using BookingHub.Application.Abstractions;
using BookingHub.Application.Abstractions.Authentication;
using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Application.Authentication.Login;
using BookingHub.Domain.Organizations;
using BookingHub.Domain.Users;
using NSubstitute;
using Xunit;

namespace BookingHub.Application.UnitTests.Authentication.Login;

public sealed class LoginHandlerTests
{
    [Fact]
    public async Task HandleWithValidCredentialsShouldReturnAccessToken()
    {
        var context = CreateContext();

        var dependencies =
            ConfigureDependencies(context);

        dependencies.PasswordHasher
            .Verify(
                "password-hash",
                "Password123!")
            .Returns(true);

        dependencies.AccessTokenProvider
            .Create(
                context.UserId,
                context.OrganizationId,
                "sergiy@example.com",
                OrganizationRole.Admin,
                context.UtcNow)
            .Returns(
                new AccessToken(
                    "access-token",
                    context.UtcNow.AddMinutes(15)));

        var handler =
            CreateHandler(dependencies);

        var result =
            await handler.HandleAsync(
                new LoginCommand(
                    " Sergiy@Example.com ",
                    "Password123!",
                    context.OrganizationId),
                TestContext.Current.CancellationToken);

        Assert.Equal(
            "access-token",
            result.AccessToken);

        Assert.Equal(
            context.UserId,
            result.UserId);

        Assert.Equal(
            context.OrganizationId,
            result.OrganizationId);

        Assert.Equal(
            OrganizationRole.Admin,
            result.Role);

        await dependencies.UserRepository
            .Received(1)
            .GetByNormalizedEmailAsync(
                "SERGIY@EXAMPLE.COM",
                TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task HandleWithInvalidPasswordShouldThrowInvalidCredentialsException()
    {
        var context = CreateContext();

        var dependencies =
            ConfigureDependencies(context);

        dependencies.PasswordHasher
            .Verify(
                "password-hash",
                "WrongPassword")
            .Returns(false);

        var handler =
            CreateHandler(dependencies);

        await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => handler.HandleAsync(
                new LoginCommand(
                    "sergiy@example.com",
                    "WrongPassword",
                    context.OrganizationId),
                TestContext.Current.CancellationToken));

        dependencies.AccessTokenProvider
            .DidNotReceive()
            .Create(
                Arg.Any<Guid>(),
                Arg.Any<Guid>(),
                Arg.Any<string>(),
                Arg.Any<OrganizationRole>(),
                Arg.Any<DateTimeOffset>());
    }

    [Fact]
    public async Task HandleWithRemovedMembershipShouldThrowInvalidCredentialsException()
    {
        var context = CreateContext();

        var dependencies =
            ConfigureDependencies(context);

        dependencies.PasswordHasher
            .Verify(
                "password-hash",
                "Password123!")
            .Returns(true);

        var removedMembership =
            OrganizationMember.Create(
                Guid.NewGuid(),
                context.OrganizationId,
                context.UserId,
                OrganizationRole.Admin,
                context.UtcNow);

        removedMembership.Remove(
            context.UtcNow.AddMinutes(1));

        dependencies.OrganizationMemberRepository
            .GetByOrganizationAndUserAsync(
                context.OrganizationId,
                context.UserId,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<OrganizationMember?>(
                    removedMembership));

        var handler =
            CreateHandler(dependencies);

        await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => handler.HandleAsync(
                new LoginCommand(
                    "sergiy@example.com",
                    "Password123!",
                    context.OrganizationId),
                TestContext.Current.CancellationToken));
    }

    private static TestDependencies ConfigureDependencies(
        LoginTestContext context)
    {
        var userRepository =
            Substitute.For<IUserRepository>();

        var organizationMemberRepository =
            Substitute.For<IOrganizationMemberRepository>();

        var organizationRepository =
            Substitute.For<IOrganizationRepository>();

        var passwordHasher =
            Substitute.For<IPasswordHasher>();

        var accessTokenProvider =
            Substitute.For<IAccessTokenProvider>();

        var clock =
            Substitute.For<IClock>();

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

        userRepository
            .GetByNormalizedEmailAsync(
                "SERGIY@EXAMPLE.COM",
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<User?>(
                    user));

        organizationMemberRepository
            .GetByOrganizationAndUserAsync(
                context.OrganizationId,
                context.UserId,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<OrganizationMember?>(
                    membership));

        organizationRepository
            .GetByIdAsync(
                context.OrganizationId,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<Organization?>(
                    organization));

        clock.UtcNow.Returns(
            context.UtcNow);

        return new TestDependencies(
            userRepository,
            organizationMemberRepository,
            organizationRepository,
            passwordHasher,
            accessTokenProvider,
            clock);
    }

    private static LoginHandler CreateHandler(
        TestDependencies dependencies)
    {
        return new LoginHandler(
            dependencies.UserRepository,
            dependencies.OrganizationMemberRepository,
            dependencies.OrganizationRepository,
            dependencies.PasswordHasher,
            dependencies.AccessTokenProvider,
            dependencies.Clock);
    }

    private static LoginTestContext CreateContext()
    {
        return new LoginTestContext(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateTimeOffset(
                2026,
                9,
                19,
                16,
                0,
                0,
                TimeSpan.Zero));
    }

    private sealed record LoginTestContext(
        Guid UserId,
        Guid OrganizationId,
        DateTimeOffset UtcNow);

    private sealed record TestDependencies(
        IUserRepository UserRepository,
        IOrganizationMemberRepository OrganizationMemberRepository,
        IOrganizationRepository OrganizationRepository,
        IPasswordHasher PasswordHasher,
        IAccessTokenProvider AccessTokenProvider,
        IClock Clock);
}
