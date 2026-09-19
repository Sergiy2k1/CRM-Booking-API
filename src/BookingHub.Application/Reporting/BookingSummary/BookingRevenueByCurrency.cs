namespace BookingHub.Application.Reporting.BookingSummary;

public sealed record BookingRevenueByCurrency(
    string Currency,
    decimal Amount);
