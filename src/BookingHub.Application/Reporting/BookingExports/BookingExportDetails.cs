using BookingHub.Domain.Reporting;

namespace BookingHub.Application.Reporting.BookingExports;

public sealed record BookingExportDetails(
    Guid Id,
    Guid OrganizationId,
    Guid RequestedByUserId,
    DateTimeOffset FromUtc,
    DateTimeOffset ToUtc,
    BookingExportStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? StartedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    int AttemptCount,
    string? LastError,
    string? FileName);
