using BookingHub.Application;
using BookingHub.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure();

var host = builder.Build();
host.Run();
