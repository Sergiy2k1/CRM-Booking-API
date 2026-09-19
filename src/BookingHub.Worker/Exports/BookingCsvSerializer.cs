using System.Globalization;
using System.Text;
using BookingHub.Application.Reporting.BookingExports;

namespace BookingHub.Worker.Exports;

internal static class BookingCsvSerializer
{
    public static string Serialize(
        IReadOnlyCollection<BookingExportRow> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);

        var builder = new StringBuilder();

        builder.AppendLine(
            "BookingId,StartsAtUtc,EndsAtUtc,Status,CustomerName,EmployeeName,ServiceName,PriceAmount,Currency,Notes");

        foreach (var row in rows)
        {
            Append(builder, row.BookingId.ToString());
            Append(builder, row.StartsAtUtc.ToUniversalTime().ToString("O"));
            Append(builder, row.EndsAtUtc.ToUniversalTime().ToString("O"));
            Append(builder, row.Status.ToString());
            Append(builder, row.CustomerName);
            Append(builder, row.EmployeeName);
            Append(builder, row.ServiceName);
            Append(builder, row.PriceAmount.ToString(CultureInfo.InvariantCulture));
            Append(builder, row.Currency);
            Append(builder, row.Notes, isLast: true);
        }

        return builder.ToString();
    }

    private static void Append(
        StringBuilder builder,
        string? value,
        bool isLast = false)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var normalized =
            ProtectSpreadsheetFormula(
                value ?? string.Empty);

        var requiresQuotes =
            normalized.Contains(',') ||
            normalized.Contains('"') ||
            normalized.Contains('\r') ||
            normalized.Contains('\n');

        if (requiresQuotes)
        {
            builder.Append('"');
            builder.Append(normalized.Replace("\"", "\"\"", StringComparison.Ordinal));
            builder.Append('"');
        }
        else
        {
            builder.Append(normalized);
        }

        if (isLast)
        {
            builder.AppendLine();
            return;
        }

        builder.Append(',');
    }

    private static string ProtectSpreadsheetFormula(
        string value)
    {
        if (value.Length == 0)
        {
            return value;
        }

        var trimmedStart =
            value.TrimStart();

        if (trimmedStart.Length == 0)
        {
            return value;
        }

        return trimmedStart[0] is '=' or '+' or '-' or '@'
            ? "'" + value
            : value;
    }
}
