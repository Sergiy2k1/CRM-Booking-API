using BookingHub.Domain.Users;
using Xunit;

namespace BookingHub.Domain.UnitTests.Users;

public sealed class UserTests
{
    [Fact]
    public void CreateWithValidDataShouldCreateActiveUser()
    {
        var id = Guid.NewGuid();

        var createdAtUtc =
            new DateTimeOffset(
                2026,
                9,
                19,
                10,
                0,
                0,
                TimeSpan.Zero);

        var user = User.Create(
            id,
            "sergiy@example.com",
            "hashed-password",
            "Sergiy",
            "Tester",
            createdAtUtc);

        Assert.Equal(id, user.Id);
        Assert.Equal("sergiy@example.com", user.Email);
        Assert.Equal("SERGIY@EXAMPLE.COM", user.NormalizedEmail);
        Assert.Equal("hashed-password", user.PasswordHash);
        Assert.Equal("Sergiy", user.FirstName);
        Assert.Equal("Tester", user.LastName);
        Assert.True(user.IsActive);
        Assert.Equal(createdAtUtc, user.CreatedAtUtc);
        Assert.Equal(createdAtUtc, user.UpdatedAtUtc);
    }

    [Fact]
    public void CreateShouldNormalizeEmail()
    {
        var user = User.Create(
            Guid.NewGuid(),
            "  Sergiy@Example.com  ",
            "hashed-password",
            "Sergiy",
            "Tester",
            DateTimeOffset.UtcNow);

        Assert.Equal(
            "Sergiy@Example.com",
            user.Email);

        Assert.Equal(
            "SERGIY@EXAMPLE.COM",
            user.NormalizedEmail);
    }

    [Fact]
    public void CreateWithEmptyEmailShouldThrowArgumentException()
    {
        var action = () => User.Create(
            Guid.NewGuid(),
            "",
            "hashed-password",
            "Sergiy",
            "Tester",
            DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void CreateWithEmptyPasswordHashShouldThrowArgumentException()
    {
        var action = () => User.Create(
            Guid.NewGuid(),
            "sergiy@example.com",
            "",
            "Sergiy",
            "Tester",
            DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void UpdateProfileShouldChangeNames()
    {
        var user = CreateUser();

        var updatedAtUtc =
            DateTimeOffset.UtcNow.AddMinutes(10);

        user.UpdateProfile(
            "New",
            "Name",
            updatedAtUtc);

        Assert.Equal("New", user.FirstName);
        Assert.Equal("Name", user.LastName);
        Assert.Equal(
            updatedAtUtc.ToUniversalTime(),
            user.UpdatedAtUtc);
    }

    [Fact]
    public void DeactivateAndActivateShouldChangeActiveState()
    {
        var user = CreateUser();

        user.Deactivate(
            DateTimeOffset.UtcNow);

        Assert.False(user.IsActive);

        user.Activate(
            DateTimeOffset.UtcNow.AddMinutes(10));

        Assert.True(user.IsActive);
    }

    private static User CreateUser()
    {
        return User.Create(
            Guid.NewGuid(),
            "sergiy@example.com",
            "hashed-password",
            "Sergiy",
            "Tester",
            DateTimeOffset.UtcNow);
    }
}
