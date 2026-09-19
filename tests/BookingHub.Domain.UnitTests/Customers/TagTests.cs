using BookingHub.Domain.Customers;
using Xunit;

namespace BookingHub.Domain.UnitTests.Customers;

public sealed class TagTests
{
    [Fact]
    public void CreateWithValidDataShouldCreateTag()
    {
        var organizationId = Guid.NewGuid();

        var createdAtUtc =
            new DateTimeOffset(
                2026,
                9,
                19,
                10,
                0,
                0,
                TimeSpan.Zero);

        var tag = Tag.Create(
            Guid.NewGuid(),
            organizationId,
            "VIP",
            createdAtUtc);

        Assert.Equal(organizationId, tag.OrganizationId);
        Assert.Equal("VIP", tag.Name);
        Assert.Equal("VIP", tag.NormalizedName);
        Assert.Equal(createdAtUtc, tag.CreatedAtUtc);
        Assert.Equal(createdAtUtc, tag.UpdatedAtUtc);
    }

    [Fact]
    public void CreateShouldNormalizeTagName()
    {
        var tag = Tag.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "  vip customer  ",
            DateTimeOffset.UtcNow);

        Assert.Equal("vip customer", tag.Name);
        Assert.Equal("VIP CUSTOMER", tag.NormalizedName);
    }

    [Fact]
    public void CreateWithEmptyOrganizationIdShouldThrowArgumentException()
    {
        var action = () => Tag.Create(
            Guid.NewGuid(),
            Guid.Empty,
            "VIP",
            DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void RenameShouldChangeNameAndNormalizedName()
    {
        var tag = Tag.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "VIP",
            DateTimeOffset.UtcNow);

        var updatedAtUtc =
            DateTimeOffset.UtcNow.AddMinutes(10);

        tag.Rename(
            "Regular",
            updatedAtUtc);

        Assert.Equal("Regular", tag.Name);
        Assert.Equal("REGULAR", tag.NormalizedName);
        Assert.Equal(
            updatedAtUtc.ToUniversalTime(),
            tag.UpdatedAtUtc);
    }
}
