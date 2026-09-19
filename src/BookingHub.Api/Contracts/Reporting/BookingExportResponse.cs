using BookingHub.Domain.Reporting;

namespace BookingHub.Api.Contracts.Reporting;

public sealed record BookingExportResponse(
    Guid Id,
    DateTimeOffset FromUtc,
    DateTimeOffset ToUtc,
    BookingExportStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? StartedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    int AttemptCount,
    string? LastError,
    string? FileName);
