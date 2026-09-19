using BookingHub.Domain.Abstractions;

namespace BookingHub.Domain.Customers;

public sealed class CustomerTag : Entity
{
    private CustomerTag(
        Guid id,
        Guid organizationId,
        Guid customerId,
        Guid tagId,
        Guid assignedByUserId,
        DateTimeOffset assignedAtUtc)
        : base(id)
    {
        OrganizationId = organizationId;
        CustomerId = customerId;
        TagId = tagId;
        AssignedByUserId = assignedByUserId;
        AssignedAtUtc = assignedAtUtc;
    }

    public Guid OrganizationId { get; private set; }

    public Guid CustomerId { get; private set; }

    public Guid TagId { get; private set; }

    public Guid AssignedByUserId { get; private set; }

    public DateTimeOffset AssignedAtUtc { get; private set; }

    public static CustomerTag Create(
        Guid id,
        Guid organizationId,
        Guid customerId,
        Guid tagId,
        Guid assignedByUserId,
        DateTimeOffset assignedAtUtc)
    {
        ValidateRelatedId(
            organizationId,
            nameof(organizationId));

        ValidateRelatedId(
            customerId,
            nameof(customerId));

        ValidateRelatedId(
            tagId,
            nameof(tagId));

        ValidateRelatedId(
            assignedByUserId,
            nameof(assignedByUserId));

        return new CustomerTag(
            id,
            organizationId,
            customerId,
            tagId,
            assignedByUserId,
            assignedAtUtc.ToUniversalTime());
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
}
