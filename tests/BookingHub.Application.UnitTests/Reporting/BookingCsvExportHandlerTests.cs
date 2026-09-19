using BookingHub.Application.Abstractions;
using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Application.Reporting.BookingExports.RequestBookingCsvExport;
using BookingHub.Domain.Reporting;
using NSubstitute;
using Xunit;

namespace BookingHub.Application.UnitTests.Reporting;

public sealed class BookingCsvExportHandlerTests
{
    [Fact]
    public async Task RequestShouldPersistPendingExport()
    {
        var repository = Substitute.For<IBookingExportRepository>();
        var guidGenerator = Substitute.For<IGuidGenerator>();
        var clock = Substitute.For<IClock>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var exportId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var now =
            new DateTimeOffset(
                2026,
                9,
                20,
                10,
                0,
                0,
                TimeSpan.Zero);

        var fromUtc = now.AddDays(-30);

        guidGenerator.NewGuid().Returns(exportId);
        clock.UtcNow.Returns(now);

        repository
            .AddAsync(
                Arg.Any<BookingExportJob>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        unitOfWork
            .SaveChangesAsync(
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var handler =
            new RequestBookingCsvExportHandler(
                repository,
                guidGenerator,
                clock,
                unitOfWork);

        var result =
            await handler.HandleAsync(
                new RequestBookingCsvExportCommand(
                    organizationId,
                    userId,
                    fromUtc,
                    now),
                TestContext.Current.CancellationToken);

        Assert.Equal(exportId, result.Id);
        Assert.Equal(BookingExportStatus.Pending, result.Status);
        Assert.Equal(organizationId, result.OrganizationId);
        Assert.Equal(userId, result.RequestedByUserId);

        await repository
            .Received(1)
            .AddAsync(
                Arg.Is<BookingExportJob>(
                    job =>
                        job.Id == exportId &&
                        job.Status == BookingExportStatus.Pending),
                TestContext.Current.CancellationToken);

        await unitOfWork
            .Received(1)
            .SaveChangesAsync(
                TestContext.Current.CancellationToken);
    }
}
