using BookingHub.Application.Abstractions.Reporting;
using BookingHub.Application.Reporting.BookingExports;
using BookingHub.Domain.Bookings;
using Dapper;
using Npgsql;

namespace BookingHub.Infrastructure.Reporting;

public sealed class DapperBookingExportReader
    : IBookingExportReader
{
    private const string ExportSql =
        """
        SELECT
            b."Id" AS "BookingId",
            b."StartsAtUtc",
            b."EndsAtUtc",
            b."Status",
            CONCAT_WS(' ', c."FirstName", c."LastName") AS "CustomerName",
            CONCAT_WS(' ', e."FirstName", e."LastName") AS "EmployeeName",
            s."Name" AS "ServiceName",
            b."PriceAmount",
            RTRIM(b."Currency") AS "Currency",
            b."Notes"
        FROM bookings AS b
        INNER JOIN customers AS c
            ON c."Id" = b."CustomerId"
        INNER JOIN employees AS e
            ON e."Id" = b."EmployeeId"
        INNER JOIN services AS s
            ON s."Id" = b."ServiceId"
        WHERE b."OrganizationId" = @OrganizationId
          AND b."StartsAtUtc" >= @FromUtc
          AND b."StartsAtUtc" < @ToUtc
        ORDER BY b."StartsAtUtc", b."Id";
        """;

    private readonly string _connectionString;

    public DapperBookingExportReader(
        string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            connectionString);

        _connectionString = connectionString;
    }

    public async Task<IReadOnlyCollection<BookingExportRow>> ReadAsync(
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
                ExportSql,
                new
                {
                    OrganizationId = organizationId,
                    FromUtc = fromUtc,
                    ToUtc = toUtc
                },
                cancellationToken: cancellationToken);

        var rows =
            await connection.QueryAsync<DatabaseBookingExportRow>(
                command);

        return rows
            .Select(Map)
            .ToArray();
    }

    private static BookingExportRow Map(
        DatabaseBookingExportRow row)
    {
        var status =
            (BookingStatus)row.Status;

        if (!Enum.IsDefined(status))
        {
            throw new InvalidOperationException(
                $"Booking status value '{row.Status}' is not supported.");
        }

        return new BookingExportRow(
            row.BookingId,
            ToUtcOffset(row.StartsAtUtc),
            ToUtcOffset(row.EndsAtUtc),
            status,
            row.CustomerName,
            row.EmployeeName,
            row.ServiceName,
            row.PriceAmount,
            row.Currency,
            row.Notes);
    }

    private static DateTimeOffset ToUtcOffset(
        DateTime value)
    {
        return new DateTimeOffset(
            DateTime.SpecifyKind(
                value,
                DateTimeKind.Utc));
    }

    private sealed class DatabaseBookingExportRow
    {
        public Guid BookingId { get; init; }

        public DateTime StartsAtUtc { get; init; }

        public DateTime EndsAtUtc { get; init; }

        public int Status { get; init; }

        public string CustomerName { get; init; } =
            string.Empty;

        public string EmployeeName { get; init; } =
            string.Empty;

        public string ServiceName { get; init; } =
            string.Empty;

        public decimal PriceAmount { get; init; }

        public string Currency { get; init; } =
            string.Empty;

        public string? Notes { get; init; }
    }
}
