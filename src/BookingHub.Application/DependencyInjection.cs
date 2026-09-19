using BookingHub.Application.Authentication.Login;
using BookingHub.Application.Authentication.RefreshSession;
using BookingHub.Application.Bookings.CreateBooking;
using BookingHub.Application.Customers.ArchiveCustomer;
using BookingHub.Application.Customers.CreateCustomer;
using BookingHub.Application.Customers.GetCustomer;
using BookingHub.Application.Customers.ListCustomers;
using BookingHub.Application.Customers.RestoreCustomer;
using BookingHub.Application.Customers.UpdateCustomer;
using BookingHub.Application.Employees.ChangeEmployeeStatus;
using BookingHub.Application.Employees.CreateEmployee;
using BookingHub.Application.Employees.GetEmployee;
using BookingHub.Application.Employees.ListEmployees;
using BookingHub.Application.Employees.Schedules.CancelTimeOff;
using BookingHub.Application.Employees.Schedules.CreateTimeOff;
using BookingHub.Application.Employees.Schedules.CreateWorkingHours;
using BookingHub.Application.Employees.Schedules.ListTimeOff;
using BookingHub.Application.Employees.Schedules.ListWorkingHours;
using BookingHub.Application.Employees.UpdateEmployee;
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

        services.AddScoped<CreateEmployeeHandler>();
        services.AddScoped<GetEmployeeHandler>();
        services.AddScoped<ListEmployeesHandler>();
        services.AddScoped<UpdateEmployeeHandler>();
        services.AddScoped<ChangeEmployeeStatusHandler>();
        services.AddScoped<CreateWorkingHoursHandler>();
        services.AddScoped<ListWorkingHoursHandler>();
        services.AddScoped<CreateTimeOffHandler>();
        services.AddScoped<ListTimeOffHandler>();
        services.AddScoped<CancelTimeOffHandler>();

        return services;
    }
}
