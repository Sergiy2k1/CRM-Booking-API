using BookingHub.Domain.Organizations;
using Xunit;

namespace BookingHub.Domain.UnitTests.Organizations;

public sealed class OrganizationTests
{
    [Fact]
    public void CreateWithValidDataShouldCreateActiveOrganization()
    {
        // Arrange
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

        // Act
        var organization = Organization.Create(
            id,
            "Beauty Studio",
            "beauty-studio",
            "Europe/Kyiv",
            createdAtUtc);

        // Assert
        Assert.Equal(id, organization.Id);
        Assert.Equal("Beauty Studio", organization.Name);
        Assert.Equal("beauty-studio", organization.Slug);
        Assert.Equal("Europe/Kyiv", organization.TimeZone);
        Assert.Equal(
            OrganizationStatus.Active,
            organization.Status);
        Assert.Equal(
            createdAtUtc,
            organization.CreatedAtUtc);
        Assert.Equal(
            createdAtUtc,
            organization.UpdatedAtUtc);
    }

    [Fact]
    public void CreateShouldNormalizeSlug()
    {
        var organization = Organization.Create(
            Guid.NewGuid(),
            "Beauty Studio",
            "  BEAUTY-STUDIO  ",
            "Europe/Kyiv",
            DateTimeOffset.UtcNow);

        Assert.Equal(
            "beauty-studio",
            organization.Slug);
    }

    [Fact]
    public void CreateWithEmptyNameShouldThrowArgumentException()
    {
        var action = () => Organization.Create(
            Guid.NewGuid(),
            "",
            "beauty-studio",
            "Europe/Kyiv",
            DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void CreateWithInvalidSlugShouldThrowArgumentException()
    {
        var action = () => Organization.Create(
            Guid.NewGuid(),
            "Beauty Studio",
            "beauty studio!",
            "Europe/Kyiv",
            DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void SuspendShouldChangeStatusToSuspended()
    {
        var organization = Organization.Create(
            Guid.NewGuid(),
            "Beauty Studio",
            "beauty-studio",
            "Europe/Kyiv",
            DateTimeOffset.UtcNow);

        var updatedAtUtc =
            DateTimeOffset.UtcNow.AddMinutes(10);

        organization.Suspend(updatedAtUtc);

        Assert.Equal(
            OrganizationStatus.Suspended,
            organization.Status);

        Assert.Equal(
            updatedAtUtc.ToUniversalTime(),
            organization.UpdatedAtUtc);
    }

    [Fact]
    public void ActivateAfterSuspensionShouldChangeStatusToActive()
    {
        var organization = Organization.Create(
            Guid.NewGuid(),
            "Beauty Studio",
            "beauty-studio",
            "Europe/Kyiv",
            DateTimeOffset.UtcNow);

        organization.Suspend(
            DateTimeOffset.UtcNow);

        organization.Activate(
            DateTimeOffset.UtcNow.AddMinutes(10));

        Assert.Equal(
            OrganizationStatus.Active,
            organization.Status);
    }
}