using BookingHub.Domain.Abstractions;

namespace BookingHub.Domain.Reporting;

public sealed class BookingExportJob : AggregateRoot
{
    public const int MaxStorageKeyLength = 500;
    public const int MaxFileNameLength = 255;
    public const int MaxErrorLength = 2000;

    private BookingExportJob(
        Guid id,
        Guid organizationId,
        Guid requestedByUserId,
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        OrganizationId = organizationId;
        RequestedByUserId = requestedByUserId;
        FromUtc = fromUtc;
        ToUtc = toUtc;
        Status = BookingExportStatus.Pending;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid OrganizationId { get; private set; }

    public Guid RequestedByUserId { get; private set; }

    public DateTimeOffset FromUtc { get; private set; }

    public DateTimeOffset ToUtc { get; private set; }

    public BookingExportStatus Status { get; private set; }

    public string? StorageKey { get; private set; }

    public string? FileName { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? StartedAtUtc { get; private set; }

    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public int AttemptCount { get; private set; }

    public string? LastError { get; private set; }

    public static BookingExportJob Create(
        Guid id,
        Guid organizationId,
        Guid requestedByUserId,
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        DateTimeOffset createdAtUtc)
    {
        ValidateId(
            organizationId,
            nameof(organizationId));

        ValidateId(
            requestedByUserId,
            nameof(requestedByUserId));

        var normalizedFromUtc =
            fromUtc.ToUniversalTime();

        var normalizedToUtc =
            toUtc.ToUniversalTime();

        if (normalizedToUtc <= normalizedFromUtc)
        {
            throw new ArgumentException(
                "Export end must be later than export start.",
                nameof(toUtc));
        }

        return new BookingExportJob(
            id,
            organizationId,
            requestedByUserId,
            normalizedFromUtc,
            normalizedToUtc,
            createdAtUtc.ToUniversalTime());
    }

    public void MarkProcessing(
        DateTimeOffset startedAtUtc)
    {
        if (Status is BookingExportStatus.Completed or
            BookingExportStatus.Failed)
        {
            throw new InvalidOperationException(
                "A completed or failed export cannot be processed again.");
        }

        Status = BookingExportStatus.Processing;
        StartedAtUtc =
            startedAtUtc.ToUniversalTime();

        AttemptCount++;
    }

    public void MarkCompleted(
        string storageKey,
        string fileName,
        DateTimeOffset completedAtUtc)
    {
        if (Status != BookingExportStatus.Processing)
        {
            throw new InvalidOperationException(
                "Only a processing export can be completed.");
        }

        StorageKey =
            NormalizeRequired(
                storageKey,
                MaxStorageKeyLength,
                nameof(storageKey));

        FileName =
            NormalizeRequired(
                fileName,
                MaxFileNameLength,
                nameof(fileName));

        Status = BookingExportStatus.Completed;
        CompletedAtUtc =
            completedAtUtc.ToUniversalTime();

        LastError = null;
    }

    public void RecordFailure(
        string error,
        int maxAttempts)
    {
        if (Status != BookingExportStatus.Processing)
        {
            throw new InvalidOperationException(
                "Only a processing export can record a failure.");
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(
            maxAttempts);

        LastError =
            NormalizeError(error);

        if (AttemptCount >= maxAttempts)
        {
            Status = BookingExportStatus.Failed;
            return;
        }

        Status = BookingExportStatus.Pending;
        StartedAtUtc = null;
    }

    private static void ValidateId(
        Guid id,
        string parameterName)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "Identifier cannot be empty.",
                parameterName);
        }
    }

    private static string NormalizeRequired(
        string value,
        int maxLength,
        string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            value,
            parameterName);

        var normalized =
            value.Trim();

        if (normalized.Length > maxLength)
        {
            throw new ArgumentException(
                $"Value cannot exceed {maxLength} characters.",
                parameterName);
        }

        return normalized;
    }

    private static string NormalizeError(
        string error)
    {
        if (string.IsNullOrWhiteSpace(error))
        {
            return "Unknown export error.";
        }

        var normalized =
            error.Trim();

        return normalized[..Math.Min(
            normalized.Length,
            MaxErrorLength)];
    }
}
