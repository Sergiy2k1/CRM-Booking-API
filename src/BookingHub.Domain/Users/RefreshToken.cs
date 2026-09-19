using BookingHub.Domain.Abstractions;

namespace BookingHub.Domain.Users;

public sealed class RefreshToken : AggregateRoot
{
    public const int MaxTokenHashLength = 128;

    private RefreshToken(
        Guid id,
        Guid userId,
        Guid organizationId,
        string tokenHash,
        DateTimeOffset expiresAtUtc,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        UserId = userId;
        OrganizationId = organizationId;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid UserId { get; private set; }

    public Guid OrganizationId { get; private set; }

    public string TokenHash { get; private set; }

    public DateTimeOffset ExpiresAtUtc { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? RevokedAtUtc { get; private set; }

    public Guid? ReplacedByTokenId { get; private set; }

    public static RefreshToken Create(
        Guid id,
        Guid userId,
        Guid organizationId,
        string tokenHash,
        DateTimeOffset expiresAtUtc,
        DateTimeOffset createdAtUtc)
    {
        ValidateRelatedId(
            userId,
            nameof(userId));

        ValidateRelatedId(
            organizationId,
            nameof(organizationId));

        ValidateTokenHash(tokenHash);

        var utcCreatedAt = createdAtUtc.ToUniversalTime();
        var utcExpiresAt = expiresAtUtc.ToUniversalTime();

        if (utcExpiresAt <= utcCreatedAt)
        {
            throw new ArgumentException(
                "Refresh token expiration must be later than creation time.",
                nameof(expiresAtUtc));
        }

        return new RefreshToken(
            id,
            userId,
            organizationId,
            tokenHash,
            utcExpiresAt,
            utcCreatedAt);
    }

    public bool IsActive(DateTimeOffset utcNow)
    {
        var normalizedUtcNow =
            utcNow.ToUniversalTime();

        return RevokedAtUtc is null &&
               normalizedUtcNow < ExpiresAtUtc;
    }

    public void Revoke(
        DateTimeOffset revokedAtUtc,
        Guid? replacedByTokenId = null)
    {
        if (RevokedAtUtc.HasValue)
        {
            return;
        }

        if (replacedByTokenId.HasValue &&
            replacedByTokenId.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "Replacement token id cannot be empty.",
                nameof(replacedByTokenId));
        }

        RevokedAtUtc =
            revokedAtUtc.ToUniversalTime();

        ReplacedByTokenId =
            replacedByTokenId;
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

    private static void ValidateTokenHash(
        string tokenHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            tokenHash);

        if (tokenHash.Length > MaxTokenHashLength)
        {
            throw new ArgumentException(
                $"Refresh token hash cannot exceed {MaxTokenHashLength} characters.",
                nameof(tokenHash));
        }
    }
}
