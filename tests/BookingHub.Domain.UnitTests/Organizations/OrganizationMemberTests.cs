using BookingHub.Domain.Organizations;
using Xunit;

namespace BookingHub.Domain.UnitTests.Organizations;

public sealed class OrganizationMemberTests
{
    [Fact]
    public void CreateWithValidDataShouldCreateActiveMember()
    {
        var organizationId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var joinedAtUtc =
            new DateTimeOffset(
                2026,
                9,
                19,
                10,
                0,
                0,
                TimeSpan.Zero);

        var member = OrganizationMember.Create(
            Guid.NewGuid(),
            organizationId,
            userId,
            OrganizationRole.Manager,
            joinedAtUtc);

        Assert.Equal(
            organizationId,
            member.OrganizationId);

        Assert.Equal(userId, member.UserId);
        Assert.Equal(OrganizationRole.Manager, member.Role);

        Assert.Equal(
            OrganizationMemberStatus.Active,
            member.Status);

        Assert.Equal(joinedAtUtc, member.JoinedAtUtc);
        Assert.Equal(joinedAtUtc, member.UpdatedAtUtc);
        Assert.Null(member.RemovedAtUtc);
    }

    [Fact]
    public void CreateWithEmptyOrganizationIdShouldThrowArgumentException()
    {
        var action = () => OrganizationMember.Create(
            Guid.NewGuid(),
            Guid.Empty,
            Guid.NewGuid(),
            OrganizationRole.Employee,
            DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void CreateWithEmptyUserIdShouldThrowArgumentException()
    {
        var action = () => OrganizationMember.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.Empty,
            OrganizationRole.Employee,
            DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void ChangeRoleShouldUpdateRole()
    {
        var member = CreateMember();

        var updatedAtUtc =
            DateTimeOffset.UtcNow.AddMinutes(10);

        member.ChangeRole(
            OrganizationRole.Admin,
            updatedAtUtc);

        Assert.Equal(
            OrganizationRole.Admin,
            member.Role);

        Assert.Equal(
            updatedAtUtc.ToUniversalTime(),
            member.UpdatedAtUtc);
    }

    [Fact]
    public void RemoveShouldMarkMemberAsRemoved()
    {
        var member = CreateMember();

        var removedAtUtc =
            DateTimeOffset.UtcNow.AddMinutes(10);

        member.Remove(removedAtUtc);

        Assert.Equal(
            OrganizationMemberStatus.Removed,
            member.Status);

        Assert.Equal(
            removedAtUtc.ToUniversalTime(),
            member.RemovedAtUtc);
    }

    [Fact]
    public void RestoreShouldReactivateRemovedMember()
    {
        var member = CreateMember();

        member.Remove(
            DateTimeOffset.UtcNow);

        member.Restore(
            DateTimeOffset.UtcNow.AddMinutes(10));

        Assert.Equal(
            OrganizationMemberStatus.Active,
            member.Status);

        Assert.Null(member.RemovedAtUtc);
    }

    [Fact]
    public void ChangeRoleForRemovedMemberShouldThrowInvalidOperationException()
    {
        var member = CreateMember();

        member.Remove(
            DateTimeOffset.UtcNow);

        var action = () => member.ChangeRole(
            OrganizationRole.Admin,
            DateTimeOffset.UtcNow.AddMinutes(10));

        Assert.Throws<InvalidOperationException>(
            action);
    }

    private static OrganizationMember CreateMember()
    {
        return OrganizationMember.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            OrganizationRole.Employee,
            DateTimeOffset.UtcNow);
    }
}
