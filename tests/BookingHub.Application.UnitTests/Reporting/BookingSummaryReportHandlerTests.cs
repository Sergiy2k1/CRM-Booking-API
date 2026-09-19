using BookingHub.Application.Abstractions.Reporting;
using BookingHub.Application.Reporting.BookingSummary;
using NSubstitute;
using Xunit;

namespace BookingHub.Application.UnitTests.Reporting;

public sealed class BookingSummaryReportHandlerTests
{
    [Fact]
    public async Task HandleWithValidPeriodShouldNormalizeUtcAndReturnReport()
    {
        var reportReader =
            Substitute.For<IBookingReportReader>();

        var organizationId = Guid.NewGuid();

        var from =
            new DateTimeOffset(
                2026,
                9,
                1,
                10,
                0,
                0,
                TimeSpan.FromHours(3));

        var to =
            from.AddDays(30);

        var expected =
            new BookingSummaryReport(
                organizationId,
                from.ToUniversalTime(),
                to.ToUniversalTime(),
                5,
                1,
                1,
                1,
                1,
                1,
                [
                    new BookingRevenueByCurrency(
                        "UAH",
                        700m)
                ]);

        reportReader
            .GetSummaryAsync(
                organizationId,
                from.ToUniversalTime(),
                to.ToUniversalTime(),
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult(expected));

        var handler =
            new GetBookingSummaryReportHandler(
                reportReader);

        var result =
            await handler.HandleAsync(
                new GetBookingSummaryReportQuery(
                    organizationId,
                    from,
                    to),
                TestContext.Current.CancellationToken);

        Assert.Equal(expected, result);

        await reportReader
            .Received(1)
            .GetSummaryAsync(
                organizationId,
                from.ToUniversalTime(),
                to.ToUniversalTime(),
                TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task HandleWithInvalidPeriodShouldThrowArgumentException()
    {
        var reportReader =
            Substitute.For<IBookingReportReader>();

        var handler =
            new GetBookingSummaryReportHandler(
                reportReader);

        var from =
            new DateTimeOffset(
                2026,
                9,
                1,
                0,
                0,
                0,
                TimeSpan.Zero);

        await Assert.ThrowsAsync<ArgumentException>(
            () => handler.HandleAsync(
                new GetBookingSummaryReportQuery(
                    Guid.NewGuid(),
                    from,
                    from),
                TestContext.Current.CancellationToken));

        await reportReader
            .DidNotReceive()
            .GetSummaryAsync(
                Arg.Any<Guid>(),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<CancellationToken>());
    }
}
