using BookingHub.Application.Authentication.Login;
using BookingHub.Application.Authentication.RefreshSession;
using BookingHub.Application.Bookings.CreateBooking;
using BookingHub.Application.Customers.ArchiveCustomer;
using BookingHub.Application.Customers.CreateCustomer;
using BookingHub.Application.Customers.GetCustomer;
using BookingHub.Application.Customers.ListCustomers;
using BookingHub.Application.Customers.RestoreCustomer;
using BookingHub.Application.Customers.UpdateCustomer;
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

        services.AddScoped<CreateCustomerHandler>();
        services.AddScoped<GetCustomerHandler>();
        services.AddScoped<ListCustomersHandler>();
        services.AddScoped<UpdateCustomerHandler>();
        services.AddScoped<ArchiveCustomerHandler>();
        services.AddScoped<RestoreCustomerHandler>();

        return services;
    }
}
