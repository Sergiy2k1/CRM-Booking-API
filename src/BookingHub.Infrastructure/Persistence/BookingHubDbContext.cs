using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Application.Authentication.RefreshSession;
using BookingHub.Application.Bookings.CreateBooking;
using BookingHub.Domain.Availability;
using BookingHub.Domain.Bookings;
using BookingHub.Domain.Customers;
using BookingHub.Domain.Employees;
using BookingHub.Domain.Organizations;
using BookingHub.Domain.Notifications;
using BookingHub.Domain.Reporting;
using BookingHub.Domain.Services;
using BookingHub.Domain.Users;
using BookingHub.Infrastructure.Messaging.Inbox;
using BookingHub.Infrastructure.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BookingHub.Infrastructure.Persistence;

public sealed class BookingHubDbContext
    : DbContext,
      IUnitOfWork
{
    public BookingHubDbContext(
        DbContextOptions<BookingHubDbContext> options)
        : base(options)
    {
    }

    public DbSet<Organization> Organizations =>
        Set<Organization>();

    public DbSet<OrganizationMember> OrganizationMembers =>
        Set<OrganizationMember>();

    public DbSet<User> Users =>
        Set<User>();

    public DbSet<RefreshToken> RefreshTokens =>
        Set<RefreshToken>();

    public DbSet<Customer> Customers =>
        Set<Customer>();

    public DbSet<Employee> Employees =>
        Set<Employee>();

    public DbSet<Service> Services =>
        Set<Service>();

    public DbSet<EmployeeService> EmployeeServices =>
        Set<EmployeeService>();

    public DbSet<EmployeeWorkingHours> EmployeeWorkingHours =>
        Set<EmployeeWorkingHours>();

    public DbSet<EmployeeTimeOff> EmployeeTimeOffPeriods =>
        Set<EmployeeTimeOff>();

    public DbSet<Booking> Bookings =>
        Set<Booking>();

    public DbSet<OutboxMessage> OutboxMessages =>
        Set<OutboxMessage>();

    public DbSet<InboxMessage> InboxMessages =>
        Set<InboxMessage>();

    public DbSet<Notification> Notifications =>
        Set<Notification>();

    public DbSet<BookingExportJob> BookingExportJobs =>
        Set<BookingExportJob>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(BookingHubDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    async Task IUnitOfWork.SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            await SaveChangesAsync(
                cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
            when (exception.Entries.Any(
                entry => entry.Entity is RefreshToken))
        {
            throw new InvalidRefreshTokenException();
        }
        catch (DbUpdateException exception)
            when (IsBookingExclusionViolation(exception))
        {
            throw new BookingUnavailableException(
                EmployeeAvailabilityStatus.BookingConflict);
        }
    }

    private static bool IsBookingExclusionViolation(
        DbUpdateException exception)
    {
        return exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.ExclusionViolation,
            ConstraintName: "EX_bookings_organization_employee_time"
        };
    }
}
