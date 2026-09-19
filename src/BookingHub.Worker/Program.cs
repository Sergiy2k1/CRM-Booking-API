using BookingHub.Application;
using BookingHub.Infrastructure;
using BookingHub.Worker.Configuration;
using BookingHub.Worker.Observability;

var builder = Host.CreateApplicationBuilder(args);

StartupConfigurationValidator.Validate(
    builder.Configuration);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddWorkerObservability(
    builder.Configuration);

builder.Services.Configure<HostOptions>(
    options =>
        options.ShutdownTimeout =
            TimeSpan.FromSeconds(30));

builder.Services.AddHostedService<BookingHub.Worker.Outbox.OutboxPublisherWorker>();
builder.Services.AddHostedService<BookingHub.Worker.Notifications.BookingNotificationConsumer>();
builder.Services.AddHostedService<BookingHub.Worker.Exports.BookingCsvExportWorker>();

var host = builder.Build();
host.Run();
