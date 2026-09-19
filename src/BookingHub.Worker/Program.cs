using BookingHub.Application;
using BookingHub.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddHostedService<BookingHub.Worker.Outbox.OutboxPublisherWorker>();

var host = builder.Build();
host.Run();
