using BookingHub.Application.Authentication.Login;
using BookingHub.Application.Authentication.RefreshSession;
using BookingHub.Application.Bookings.CreateBooking;
using Microsoft.Extensions.DependencyInjection;

namespace BookingHub.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<CreateBookingHandler>();
        services.AddScoped<LoginHandler>();
        services.AddScoped<RefreshSessionHandler>();

        return services;
    }
}
