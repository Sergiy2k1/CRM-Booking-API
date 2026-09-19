using BookingHub.Domain.Organizations;
using BookingHub.Domain.Reporting;
using BookingHub.Domain.Users;
using BookingHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace BookingHub.Infrastructure.IntegrationTests.Reporting;

public sealed class BookingCsvExportPersistenceTests
{
    [Fact]
    public async Task ExportMigrationShouldPersistPendingJob()
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
        var userId = Guid.NewGuid();

        var createdAtUtc =
            new DateTimeOffset(
                2026,
                9,
                20,
                10,
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
            User.Create(
                userId,
                "export@example.com",
                "password-hash",
                "Export",
                "User",
                createdAtUtc));

        await dbContext.SaveChangesAsync(
            TestContext.Current.CancellationToken);

        var exportJob =
            BookingExportJob.Create(
                Guid.NewGuid(),
                organizationId,
                userId,
                createdAtUtc.AddDays(-30),
                createdAtUtc,
                createdAtUtc);

        dbContext.BookingExportJobs.Add(
            exportJob);

        await dbContext.SaveChangesAsync(
            TestContext.Current.CancellationToken);

        var persisted =
            await dbContext.BookingExportJobs
                .AsNoTracking()
                .SingleAsync(
                    x => x.Id == exportJob.Id,
                    TestContext.Current.CancellationToken);

        Assert.Equal(
            BookingExportStatus.Pending,
            persisted.Status);

        Assert.Equal(
            organizationId,
            persisted.OrganizationId);

        Assert.Equal(
            userId,
            persisted.RequestedByUserId);

        Assert.Equal(
            0,
            persisted.AttemptCount);

        Assert.Null(
            persisted.StorageKey);
    }
}
