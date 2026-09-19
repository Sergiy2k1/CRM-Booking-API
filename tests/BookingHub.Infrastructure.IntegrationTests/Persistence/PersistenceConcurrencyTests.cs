using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Application.Authentication.RefreshSession;
using BookingHub.Application.Bookings.CreateBooking;
using BookingHub.Domain.Availability;
using BookingHub.Domain.Bookings;
using BookingHub.Domain.Customers;
using BookingHub.Domain.Employees;
using BookingHub.Domain.Organizations;
using BookingHub.Domain.Services;
using BookingHub.Domain.Users;
using BookingHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace BookingHub.Infrastructure.IntegrationTests.Persistence;

public sealed class PersistenceConcurrencyTests
{
    [Fact]
    public async Task ConcurrentRefreshRotationShouldRejectSecondSave()
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

        var organizationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var tokenId = Guid.NewGuid();

        var createdAtUtc =
            new DateTimeOffset(
                2026,
                9,
                20,
                10,
                0,
                0,
                TimeSpan.Zero);

        await using (var setupContext =
                     new BookingHubDbContext(options))
        {
            await setupContext.Database.MigrateAsync(
                TestContext.Current.CancellationToken);

            setupContext.AddRange(
                Organization.Create(
                    organizationId,
                    "Beauty Studio",
                    "beauty-studio",
                    "UTC",
                    createdAtUtc),
                User.Create(
                    userId,
                    "refresh@example.com",
                    "password-hash",
                    "Refresh",
                    "User",
                    createdAtUtc),
                RefreshToken.Create(
                    tokenId,
                    userId,
                    organizationId,
                    "refresh-token-hash",
                    createdAtUtc.AddDays(30),
                    createdAtUtc));

            await setupContext.SaveChangesAsync(
                TestContext.Current.CancellationToken);
        }

        await using var firstContext =
            new BookingHubDbContext(options);

        await using var secondContext =
            new BookingHubDbContext(options);

        var firstToken =
            await firstContext.RefreshTokens
                .SingleAsync(
                    x => x.Id == tokenId,
                    TestContext.Current.CancellationToken);

        var secondToken =
            await secondContext.RefreshTokens
                .SingleAsync(
                    x => x.Id == tokenId,
                    TestContext.Current.CancellationToken);

        firstToken.Revoke(
            createdAtUtc.AddMinutes(1),
            Guid.NewGuid());

        secondToken.Revoke(
            createdAtUtc.AddMinutes(2),
            Guid.NewGuid());

        await ((IUnitOfWork)firstContext)
            .SaveChangesAsync(
                TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<InvalidRefreshTokenException>(
            () =>
                ((IUnitOfWork)secondContext)
                    .SaveChangesAsync(
                        TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task BookingExclusionViolationShouldBecomeBookingUnavailable()
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

        var organizationId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();

        var createdAtUtc =
            new DateTimeOffset(
                2026,
                9,
                20,
                10,
                0,
                0,
                TimeSpan.Zero);

        var startsAtUtc =
            createdAtUtc.AddDays(1);

        await using (var setupContext =
                     new BookingHubDbContext(options))
        {
            await setupContext.Database.MigrateAsync(
                TestContext.Current.CancellationToken);

            setupContext.AddRange(
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

            await setupContext.SaveChangesAsync(
                TestContext.Current.CancellationToken);

            setupContext.Bookings.Add(
                Booking.Create(
                    Guid.NewGuid(),
                    organizationId,
                    customerId,
                    employeeId,
                    serviceId,
                    startsAtUtc,
                    startsAtUtc.AddHours(1),
                    700m,
                    "UAH",
                    null,
                    createdAtUtc));

            await setupContext.SaveChangesAsync(
                TestContext.Current.CancellationToken);
        }

        await using var conflictingContext =
            new BookingHubDbContext(options);

        conflictingContext.Bookings.Add(
            Booking.Create(
                Guid.NewGuid(),
                organizationId,
                customerId,
                employeeId,
                serviceId,
                startsAtUtc.AddMinutes(30),
                startsAtUtc.AddMinutes(90),
                700m,
                "UAH",
                null,
                createdAtUtc));

        var exception =
            await Assert.ThrowsAsync<BookingUnavailableException>(
                () =>
                    ((IUnitOfWork)conflictingContext)
                        .SaveChangesAsync(
                            TestContext.Current.CancellationToken));

        Assert.Equal(
            EmployeeAvailabilityStatus.BookingConflict,
            exception.AvailabilityStatus);
    }
    [Fact]
    public async Task ConcurrentBookingUpdateShouldRejectSecondWriter()
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
                10,
                0,
                0,
                TimeSpan.Zero);

        var startsAtUtc =
            createdAtUtc.AddDays(1);

        await using (var setupContext =
                     new BookingHubDbContext(options))
        {
            await setupContext.Database.MigrateAsync(
                TestContext.Current.CancellationToken);

            setupContext.AddRange(
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

            await setupContext.SaveChangesAsync(
                TestContext.Current.CancellationToken);

            setupContext.Bookings.Add(
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
                    null,
                    createdAtUtc));

            await setupContext.SaveChangesAsync(
                TestContext.Current.CancellationToken);
        }

        await using var firstContext =
            new BookingHubDbContext(options);

        await using var secondContext =
            new BookingHubDbContext(options);

        var firstBooking =
            await firstContext.Bookings
                .SingleAsync(
                    x => x.Id == bookingId,
                    TestContext.Current.CancellationToken);

        var secondBooking =
            await secondContext.Bookings
                .SingleAsync(
                    x => x.Id == bookingId,
                    TestContext.Current.CancellationToken);

        firstBooking.Confirm(
            createdAtUtc.AddMinutes(1));

        secondBooking.Cancel(
            createdAtUtc.AddMinutes(2));

        await ((IUnitOfWork)firstContext)
            .SaveChangesAsync(
                TestContext.Current.CancellationToken);

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () =>
                    ((IUnitOfWork)secondContext)
                        .SaveChangesAsync(
                            TestContext.Current.CancellationToken));

        Assert.Contains(
            "modified concurrently",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

}
