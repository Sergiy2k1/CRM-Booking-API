using BookingHub.Domain.Abstractions;

namespace BookingHub.Domain.Customers;

public sealed class CustomerNote : AggregateRoot
{
    public const int MaxContentLength = 4000;

    private CustomerNote(
        Guid id,
        Guid organizationId,
        Guid customerId,
        Guid authorUserId,
        string content,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        OrganizationId = organizationId;
        CustomerId = customerId;
        AuthorUserId = authorUserId;
        Content = content;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public Guid OrganizationId { get; private set; }

    public Guid CustomerId { get; private set; }

    public Guid AuthorUserId { get; private set; }

    public string Content { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static CustomerNote Create(
        Guid id,
        Guid organizationId,
        Guid customerId,
        Guid authorUserId,
        string content,
        DateTimeOffset createdAtUtc)
    {
        ValidateRelatedId(
            organizationId,
            nameof(organizationId));

        ValidateRelatedId(
            customerId,
            nameof(customerId));

        ValidateRelatedId(
            authorUserId,
            nameof(authorUserId));

        ValidateContent(content);

        var utcCreatedAt = createdAtUtc.ToUniversalTime();

        return new CustomerNote(
            id,
            organizationId,
            customerId,
            authorUserId,
            content.Trim(),
            utcCreatedAt);
    }

    public void UpdateContent(
        string content,
        DateTimeOffset updatedAtUtc)
    {
        ValidateContent(content);

        Content = content.Trim();
        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
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

    private static void ValidateContent(string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        if (content.Trim().Length > MaxContentLength)
        {
            throw new ArgumentException(
                $"Customer note cannot exceed {MaxContentLength} characters.",
                nameof(content));
        }
    }
}
