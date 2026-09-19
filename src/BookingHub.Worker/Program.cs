using BookingHub.Application;
using BookingHub.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddHostedService<BookingHub.Worker.Outbox.OutboxPublisherWorker>();
builder.Services.AddHostedService<BookingHub.Worker.Notifications.BookingNotificationConsumer>();
builder.Services.AddHostedService<BookingHub.Worker.Exports.BookingCsvExportWorker>();

var host = builder.Build();
host.Run();
