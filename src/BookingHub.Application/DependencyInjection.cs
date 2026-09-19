using BookingHub.Application.Bookings.CreateBooking;
using Microsoft.Extensions.DependencyInjection;

namespace BookingHub.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<CreateBookingHandler>();

        return services;
    }
}
