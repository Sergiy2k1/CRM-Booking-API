using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Domain.Bookings;
using BookingHub.Domain.Customers;
using BookingHub.Domain.Employees;
using BookingHub.Domain.Organizations;
using BookingHub.Domain.Services;
using BookingHub.Domain.Users;
using BookingHub.Infrastructure.Messaging.Inbox;
using BookingHub.Infrastructure.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;

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
        await SaveChangesAsync(cancellationToken);
    }
}
