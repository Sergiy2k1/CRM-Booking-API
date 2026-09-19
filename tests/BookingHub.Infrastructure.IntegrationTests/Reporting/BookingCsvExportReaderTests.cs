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

public sealed class BookingCsvExportReaderTests
{
    [Fact]
    public async Task ReadShouldReturnTenantBookingRowsWithJoinedNames()
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
        var bookingId = Guid.NewGuid();

        var createdAtUtc =
            new DateTimeOffset(
                2026,
                9,
                20,
                9,
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
                "Anna",
                "Client",
                "anna@example.com",
                "+380501111111",
                createdAtUtc),
            Employee.Create(
                employeeId,
                organizationId,
                null,
                "Sergiy",
                "Barber",
                "Senior barber",
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

        var startsAtUtc =
            createdAtUtc.AddDays(1);

        dbContext.Bookings.Add(
            Booking.Create(
                bookingId,
                organizationId,
                customerId,
                employeeId,
                serviceId,
                startsAtUtc,
                startsAtUtc.AddHours(1),
                700m,
                "UAH",
                "First visit",
                createdAtUtc));

        await dbContext.SaveChangesAsync(
            TestContext.Current.CancellationToken);

        var reader =
            new DapperBookingExportReader(
                connectionString);

        var rows =
            await reader.ReadAsync(
                organizationId,
                startsAtUtc.AddHours(-1),
                startsAtUtc.AddHours(2),
                TestContext.Current.CancellationToken);

        var row =
            Assert.Single(rows);

        Assert.Equal(bookingId, row.BookingId);
        Assert.Equal(startsAtUtc, row.StartsAtUtc);
        Assert.Equal(startsAtUtc.AddHours(1), row.EndsAtUtc);
        Assert.Equal("Anna Client", row.CustomerName);
        Assert.Equal("Sergiy Barber", row.EmployeeName);
        Assert.Equal("Haircut", row.ServiceName);
        Assert.Equal(BookingStatus.Pending, row.Status);
        Assert.Equal(700m, row.PriceAmount);
        Assert.Equal("UAH", row.Currency);
        Assert.Equal("First visit", row.Notes);
    }
}
