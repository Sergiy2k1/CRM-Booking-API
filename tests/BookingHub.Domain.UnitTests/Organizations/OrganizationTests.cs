using BookingHub.Domain.Organizations;
using Xunit;

namespace BookingHub.Domain.UnitTests.Organizations;

public sealed class OrganizationTests
{
    [Fact]
    public void CreateWithValidDataShouldCreateActiveOrganization()
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

        var organization = Organization.Create(
            id,
            "Beauty Studio",
            "beauty-studio",
            "Europe/Kyiv",
            createdAtUtc);

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
        var createdAtUtc = DateTimeOffset.UtcNow;

        var organization = Organization.Create(
            Guid.NewGuid(),
            "Beauty Studio",
            "  BEAUTY-STUDIO  ",
            "Europe/Kyiv",
            createdAtUtc);

        Assert.Equal(
            "beauty-studio",
            organization.Slug);
    }

    [Fact]
    public void CreateWithEmptyNameShouldThrowArgumentException()
    {
        var createdAtUtc = DateTimeOffset.UtcNow;

        var action = () => Organization.Create(
            Guid.NewGuid(),
            "",
            "beauty-studio",
            "Europe/Kyiv",
            createdAtUtc);

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void CreateWithInvalidSlugShouldThrowArgumentException()
    {
        var createdAtUtc = DateTimeOffset.UtcNow;

        var action = () => Organization.Create(
            Guid.NewGuid(),
            "Beauty Studio",
            "beauty studio!",
            "Europe/Kyiv",
            createdAtUtc);

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void CreateWithEmptyIdShouldThrowArgumentException()
    {
        var createdAtUtc = DateTimeOffset.UtcNow;

        var action = () => Organization.Create(
            Guid.Empty,
            "Beauty Studio",
            "beauty-studio",
            "Europe/Kyiv",
            createdAtUtc);

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