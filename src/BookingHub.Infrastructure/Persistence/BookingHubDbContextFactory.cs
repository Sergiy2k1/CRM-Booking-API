using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BookingHub.Infrastructure.Persistence;

public sealed class BookingHubDbContextFactory
    : IDesignTimeDbContextFactory<BookingHubDbContext>
{
    private const string DefaultConnectionString =
        "Host=localhost;Port=5432;Database=bookinghub;Username=bookinghub;Password=bookinghub";

    public BookingHubDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable(
                "BOOKINGHUB_CONNECTION_STRING")
            ?? DefaultConnectionString;

        var optionsBuilder =
            new DbContextOptionsBuilder<BookingHubDbContext>();

        optionsBuilder.UseNpgsql(connectionString);

        return new BookingHubDbContext(
            optionsBuilder.Options);
    }
}
