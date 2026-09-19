using BookingHub.Application.Abstractions;
using BookingHub.Application.Abstractions.Authentication;
using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Infrastructure.Authentication;
using BookingHub.Infrastructure.Identifiers;
using BookingHub.Infrastructure.Persistence;
using BookingHub.Infrastructure.Persistence.Repositories;
using BookingHub.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookingHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException(
                "Database connection string is not configured.");

        services.AddDbContext<BookingHubDbContext>(
            options =>
                options.UseNpgsql(connectionString));

        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<IOrganizationMemberRepository, OrganizationMemberRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<IEmployeeServiceRepository, EmployeeServiceRepository>();
        services.AddScoped<IEmployeeScheduleRepository, EmployeeScheduleRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();

        services.AddScoped<IUnitOfWork>(
            serviceProvider =>
                serviceProvider.GetRequiredService<BookingHubDbContext>());

        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IGuidGenerator, GuidGenerator>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IAccessTokenProvider, JwtAccessTokenProvider>();
        services.AddSingleton<IRefreshTokenProvider, RefreshTokenProvider>();

        return services;
    }
}
