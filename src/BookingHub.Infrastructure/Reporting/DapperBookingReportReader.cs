using BookingHub.Application.Abstractions.Reporting;
using BookingHub.Application.Reporting.BookingSummary;
using Dapper;
using Npgsql;

namespace BookingHub.Infrastructure.Reporting;

public sealed class DapperBookingReportReader
    : IBookingReportReader
{
    private const string SummarySql =
        """
        SELECT
            COUNT(*)::int AS "TotalBookings",
            COUNT(*) FILTER (WHERE "Status" = 1)::int AS "PendingCount",
            COUNT(*) FILTER (WHERE "Status" = 2)::int AS "ConfirmedCount",
            COUNT(*) FILTER (WHERE "Status" = 3)::int AS "CompletedCount",
            COUNT(*) FILTER (WHERE "Status" = 4)::int AS "CancelledCount",
            COUNT(*) FILTER (WHERE "Status" = 5)::int AS "NoShowCount"
        FROM bookings
        WHERE "OrganizationId" = @OrganizationId
          AND "StartsAtUtc" >= @FromUtc
          AND "StartsAtUtc" < @ToUtc;

        SELECT
            RTRIM("Currency") AS "Currency",
            SUM("PriceAmount") AS "Amount"
        FROM bookings
        WHERE "OrganizationId" = @OrganizationId
          AND "StartsAtUtc" >= @FromUtc
          AND "StartsAtUtc" < @ToUtc
          AND "Status" = 3
        GROUP BY "Currency"
        ORDER BY "Currency";
        """;

    private readonly string _connectionString;

    public DapperBookingReportReader(
        string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            connectionString);

        _connectionString = connectionString;
    }

    public async Task<BookingSummaryReport> GetSummaryAsync(
        Guid organizationId,
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        CancellationToken cancellationToken = default)
    {
        await using var connection =
            new NpgsqlConnection(
                _connectionString);

        await connection.OpenAsync(
            cancellationToken);

        var command =
            new CommandDefinition(
                SummarySql,
                new
                {
                    OrganizationId = organizationId,
                    FromUtc = fromUtc,
                    ToUtc = toUtc
                },
                cancellationToken: cancellationToken);

        using var result =
            await connection.QueryMultipleAsync(
                command);

        var summary =
            await result.ReadSingleAsync<SummaryRow>();

        var revenue =
            (await result.ReadAsync<RevenueRow>())
                .Select(
                    x => new BookingRevenueByCurrency(
                        x.Currency,
                        x.Amount))
                .ToArray();

        return new BookingSummaryReport(
            organizationId,
            fromUtc,
            toUtc,
            summary.TotalBookings,
            summary.PendingCount,
            summary.ConfirmedCount,
            summary.CompletedCount,
            summary.CancelledCount,
            summary.NoShowCount,
            revenue);
    }

    private sealed class SummaryRow
    {
        public int TotalBookings { get; init; }

        public int PendingCount { get; init; }

        public int ConfirmedCount { get; init; }

        public int CompletedCount { get; init; }

        public int CancelledCount { get; init; }

        public int NoShowCount { get; init; }
    }

    private sealed class RevenueRow
    {
        public string Currency { get; init; } =
            string.Empty;

        public decimal Amount { get; init; }
    }
}
