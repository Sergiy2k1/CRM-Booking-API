using BookingHub.Domain.Bookings;
using BookingHub.Domain.Customers;
using BookingHub.Domain.Employees;
using BookingHub.Domain.Organizations;
using BookingHub.Domain.Services;
using BookingHub.Infrastructure.Persistence;
using BookingHub.Infrastructure.Reporting;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace BookingHub.Infrastructure.IntegrationTests.Reporting;

public sealed class BookingSummaryReportTests
{
    [Fact]
    public async Task SummaryShouldAggregateStatusesAndCompletedRevenueByCurrency()
    {
        await using var postgreSqlContainer =
            new PostgreSqlBuilder("postgres:17-alpine")
                .WithDatabase("bookinghub")
                .WithUsername("bookinghub")
                .WithPassword("bookinghub")
                .Build();

        await postgreSqlContainer.StartAsync(
            TestContext.Current.CancellationToken);

        var connectionString =
            postgreSqlContainer.GetConnectionString();

        var options =
            new DbContextOptionsBuilder<BookingHubDbContext>()
                .UseNpgsql(connectionString)
                .Options;

        await using var dbContext =
            new BookingHubDbContext(options);

        await dbContext.Database.MigrateAsync(
            TestContext.Current.CancellationToken);

        var organizationId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();

        var createdAtUtc =
            new DateTimeOffset(
                2026,
                8,
                31,
                12,
                0,
                0,
                TimeSpan.Zero);

        dbContext.AddRange(
            Organization.Create(
                organizationId,
                "Beauty Studio",
                "beauty-studio",
                "UTC",
                createdAtUtc),
            Customer.Create(
                customerId,
                organizationId,
                "Sergiy",
                "Tester",
                "sergiy@example.com",
                "+380501234567",
                createdAtUtc),
            Employee.Create(
                employeeId,
                organizationId,
                null,
                "Sergiy",
                "Tester",
                "Barber",
                createdAtUtc),
            Service.Create(
                serviceId,
                organizationId,
                "Haircut",
                null,
                TimeSpan.FromHours(1),
                700m,
                "UAH",
                createdAtUtc));

        await dbContext.SaveChangesAsync(
            TestContext.Current.CancellationToken);

        var periodStart =
            new DateTimeOffset(
                2026,
                9,
                1,
                0,
                0,
                0,
                TimeSpan.Zero);

        var pending =
            CreateBooking(
                organizationId,
                customerId,
                employeeId,
                serviceId,
                periodStart.AddHours(10),
                500m,
                "UAH",
                createdAtUtc);

        var completedUah =
            CreateBooking(
                organizationId,
                customerId,
                employeeId,
                serviceId,
                periodStart.AddHours(12),
                700m,
                "UAH",
                createdAtUtc);

        completedUah.Confirm(
            createdAtUtc.AddMinutes(1));

        completedUah.Complete(
            createdAtUtc.AddMinutes(2));

        var completedUsd =
            CreateBooking(
                organizationId,
                customerId,
                employeeId,
                serviceId,
                periodStart.AddHours(14),
                25m,
                "USD",
                createdAtUtc);

        completedUsd.Confirm(
            createdAtUtc.AddMinutes(3));

        completedUsd.Complete(
            createdAtUtc.AddMinutes(4));

        var cancelled =
            CreateBooking(
                organizationId,
                customerId,
                employeeId,
                serviceId,
                periodStart.AddHours(16),
                900m,
                "UAH",
                createdAtUtc);

        cancelled.Cancel(
            createdAtUtc.AddMinutes(5));

        var outsidePeriod =
            CreateBooking(
                organizationId,
                customerId,
                employeeId,
                serviceId,
                periodStart.AddDays(2),
                1000m,
                "UAH",
                createdAtUtc);

        dbContext.Bookings.AddRange(
            pending,
            completedUah,
            completedUsd,
            cancelled,
            outsidePeriod);

        await dbContext.SaveChangesAsync(
            TestContext.Current.CancellationToken);

        var reader =
            new DapperBookingReportReader(
                connectionString);

        var report =
            await reader.GetSummaryAsync(
                organizationId,
                periodStart,
                periodStart.AddDays(1),
                TestContext.Current.CancellationToken);

        Assert.Equal(4, report.TotalBookings);
        Assert.Equal(1, report.PendingCount);
        Assert.Equal(0, report.ConfirmedCount);
        Assert.Equal(2, report.CompletedCount);
        Assert.Equal(1, report.CancelledCount);
        Assert.Equal(0, report.NoShowCount);

        Assert.Collection(
            report.CompletedRevenue,
            usd =>
            {
                Assert.Equal("UAH", usd.Currency);
                Assert.Equal(700m, usd.Amount);
            },
            usd =>
            {
                Assert.Equal("USD", usd.Currency);
                Assert.Equal(25m, usd.Amount);
            });
    }

    private static Booking CreateBooking(
        Guid organizationId,
        Guid customerId,
        Guid employeeId,
        Guid serviceId,
        DateTimeOffset startsAtUtc,
        decimal price,
        string currency,
        DateTimeOffset createdAtUtc)
    {
        return Booking.Create(
            Guid.NewGuid(),
            organizationId,
            customerId,
            employeeId,
            serviceId,
            startsAtUtc,
            startsAtUtc.AddHours(1),
            price,
            currency,
            null,
            createdAtUtc);
    }
}
