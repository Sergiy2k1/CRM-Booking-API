using BookingHub.Domain.Abstractions;

namespace BookingHub.Domain.Organizations;

public sealed class OrganizationMember : AggregateRoot
{
    private OrganizationMember(
        Guid id,
        Guid organizationId,
        Guid userId,
        OrganizationRole role,
        DateTimeOffset joinedAtUtc)
        : base(id)
    {
        OrganizationId = organizationId;
        UserId = userId;
        Role = role;
        Status = OrganizationMemberStatus.Active;
        JoinedAtUtc = joinedAtUtc;
        UpdatedAtUtc = joinedAtUtc;
    }

    public Guid OrganizationId { get; private set; }

    public Guid UserId { get; private set; }

    public OrganizationRole Role { get; private set; }

    public OrganizationMemberStatus Status { get; private set; }

    public DateTimeOffset JoinedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public DateTimeOffset? RemovedAtUtc { get; private set; }

    public static OrganizationMember Create(
        Guid id,
        Guid organizationId,
        Guid userId,
        OrganizationRole role,
        DateTimeOffset joinedAtUtc)
    {
        ValidateRelatedId(
            organizationId,
            nameof(organizationId));

        ValidateRelatedId(
            userId,
            nameof(userId));

        ValidateRole(role);

        return new OrganizationMember(
            id,
            organizationId,
            userId,
            role,
            joinedAtUtc.ToUniversalTime());
    }

    public void ChangeRole(
        OrganizationRole role,
        DateTimeOffset updatedAtUtc)
    {
        if (Status == OrganizationMemberStatus.Removed)
        {
            throw new InvalidOperationException(
                "Removed organization member cannot change role.");
        }

        ValidateRole(role);

        if (Role == role)
        {
            return;
        }

        Role = role;
        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
    }

    public void Remove(DateTimeOffset removedAtUtc)
    {
        if (Status == OrganizationMemberStatus.Removed)
        {
            return;
        }

        var utcRemovedAt = removedAtUtc.ToUniversalTime();

        Status = OrganizationMemberStatus.Removed;
        RemovedAtUtc = utcRemovedAt;
        UpdatedAtUtc = utcRemovedAt;
    }

    public void Restore(DateTimeOffset restoredAtUtc)
    {
        if (Status == OrganizationMemberStatus.Active)
        {
            return;
        }

        Status = OrganizationMemberStatus.Active;
        RemovedAtUtc = null;
        UpdatedAtUtc = restoredAtUtc.ToUniversalTime();
    }

    private static void ValidateRelatedId(
        Guid id,
        string parameterName)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "Related entity id cannot be empty.",
                parameterName);
        }
    }

    private static void ValidateRole(OrganizationRole role)
    {
        if (!Enum.IsDefined(role))
        {
            throw new ArgumentOutOfRangeException(
                nameof(role),
                role,
                "Unsupported organization role.");
        }
    }
}
