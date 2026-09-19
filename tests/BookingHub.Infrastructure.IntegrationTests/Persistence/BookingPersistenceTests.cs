using System.Text.Json;
using BookingHub.Domain.Bookings;
using BookingHub.Domain.Customers;
using BookingHub.Domain.Employees;
using BookingHub.Domain.Organizations;
using BookingHub.Domain.Services;
using BookingHub.Infrastructure.Persistence;
using BookingHub.Infrastructure.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace BookingHub.Infrastructure.IntegrationTests.Persistence;

public sealed class BookingPersistenceTests
{
    [Fact]
    public async Task OutboxMigrationShouldPersistJsonPayload()
    {
        await using var postgreSqlContainer =
            new PostgreSqlBuilder("postgres:17-alpine")
                .WithDatabase("bookinghub")
                .WithUsername("bookinghub")
                .WithPassword("bookinghub")
                .Build();

        await postgreSqlContainer.StartAsync(
            TestContext.Current.CancellationToken);

        var options =
            new DbContextOptionsBuilder<BookingHubDbContext>()
                .UseNpgsql(
                    postgreSqlContainer.GetConnectionString())
                .Options;

        await using var dbContext =
            new BookingHubDbContext(options);

        await dbContext.Database.MigrateAsync(
            TestContext.Current.CancellationToken);

        var occurredAtUtc =
            new DateTimeOffset(
                2026,
                9,
                19,
                20,
                30,
                0,
                TimeSpan.Zero);

        var message =
            OutboxMessage.Create(
                Guid.NewGuid(),
                "booking.created",
                "{\"bookingId\":\"test\"}",
                occurredAtUtc);

        dbContext.OutboxMessages.Add(message);

        await dbContext.SaveChangesAsync(
            TestContext.Current.CancellationToken);

        var persisted =
            await dbContext.OutboxMessages
                .AsNoTracking()
                .SingleAsync(
                    x => x.Id == message.Id,
                    TestContext.Current.CancellationToken);

        Assert.Equal("booking.created", persisted.Type);

        using var payloadDocument =
            JsonDocument.Parse(
                persisted.Payload);

        Assert.Equal(
            "test",
            payloadDocument.RootElement
                .GetProperty("bookingId")
                .GetString());

        Assert.Null(persisted.ProcessedAtUtc);
        Assert.Equal(0, persisted.AttemptCount);
    }

    [Fact]
    public async Task MigrationAndDoubleBookingConstraintShouldWork()
    {
        await using var postgreSqlContainer =
            new PostgreSqlBuilder("postgres:17-alpine")
                .WithDatabase("bookinghub")
                .WithUsername("bookinghub")
                .WithPassword("bookinghub")
                .Build();

        await postgreSqlContainer.StartAsync(
            TestContext.Current.CancellationToken);

        var options =
            new DbContextOptionsBuilder<BookingHubDbContext>()
                .UseNpgsql(
                    postgreSqlContainer.GetConnectionString())
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
                9,
                19,
                12,
                0,
                0,
                TimeSpan.Zero);

        var organization =
            Organization.Create(
                organizationId,
                "Beauty Studio",
                "beauty-studio",
                "UTC",
                createdAtUtc);

        var customer =
            Customer.Create(
                customerId,
                organizationId,
                "Sergiy",
                "Tester",
                "sergiy@example.com",
                "+380501234567",
                createdAtUtc);

        var employee =
            Employee.Create(
                employeeId,
                organizationId,
                null,
                "Sergiy",
                "Tester",
                "Barber",
                createdAtUtc);

        var service =
            Service.Create(
                serviceId,
                organizationId,
                "Haircut",
                "Classic haircut",
                TimeSpan.FromHours(1),
                700m,
                "UAH",
                createdAtUtc);

        dbContext.AddRange(
            organization,
            customer,
            employee,
            service);

        await dbContext.SaveChangesAsync(
            TestContext.Current.CancellationToken);

        var firstStartsAtUtc =
            new DateTimeOffset(
                2026,
                9,
                21,
                10,
                0,
                0,
                TimeSpan.Zero);

        var firstBooking =
            Booking.Create(
                Guid.NewGuid(),
                organizationId,
                customerId,
                employeeId,
                serviceId,
                firstStartsAtUtc,
                firstStartsAtUtc.AddHours(1),
                700m,
                "UAH",
                null,
                createdAtUtc);

        dbContext.Bookings.Add(firstBooking);

        await dbContext.SaveChangesAsync(
            TestContext.Current.CancellationToken);

        var adjacentBooking =
            Booking.Create(
                Guid.NewGuid(),
                organizationId,
                customerId,
                employeeId,
                serviceId,
                firstStartsAtUtc.AddHours(1),
                firstStartsAtUtc.AddHours(2),
                700m,
                "UAH",
                null,
                createdAtUtc);

        dbContext.Bookings.Add(adjacentBooking);

        await dbContext.SaveChangesAsync(
            TestContext.Current.CancellationToken);

        var persistedBooking =
            await dbContext.Bookings
                .AsNoTracking()
                .SingleAsync(
                    x => x.Id == firstBooking.Id,
                    TestContext.Current.CancellationToken);

        Assert.Equal(
            firstBooking.StartsAtUtc,
            persistedBooking.StartsAtUtc);

        var overlappingBooking =
            Booking.Create(
                Guid.NewGuid(),
                organizationId,
                customerId,
                employeeId,
                serviceId,
                firstStartsAtUtc.AddMinutes(30),
                firstStartsAtUtc.AddMinutes(90),
                700m,
                "UAH",
                null,
                createdAtUtc);

        dbContext.Bookings.Add(overlappingBooking);

        var exception =
            await Assert.ThrowsAsync<DbUpdateException>(
                async () =>
                {
                    await dbContext.SaveChangesAsync(
                        TestContext.Current.CancellationToken);
                });

        var postgresException =
            Assert.IsType<PostgresException>(
                exception.InnerException);

        Assert.Equal(
            "23P01",
            postgresException.SqlState);
    }
}
