namespace BookingHub.Worker.Configuration;

internal static class StartupConfigurationValidator
{
    public static void Validate(
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var errors =
            new List<string>();

        Require(
            configuration.GetConnectionString("Database"),
            "ConnectionStrings:Database",
            errors);

        Require(
            configuration["RabbitMq:HostName"],
            "RabbitMq:HostName",
            errors);

        RequirePositiveInt(
            configuration["RabbitMq:Port"],
            "RabbitMq:Port",
            errors);

        Require(
            configuration["RabbitMq:UserName"],
            "RabbitMq:UserName",
            errors);

        Require(
            configuration["RabbitMq:Password"],
            "RabbitMq:Password",
            errors);

        Require(
            configuration["RabbitMq:Exchange"],
            "RabbitMq:Exchange",
            errors);

        Require(
            configuration["RabbitMq:NotificationQueue"],
            "RabbitMq:NotificationQueue",
            errors);

        Require(
            configuration["ExportStorage:RootPath"],
            "ExportStorage:RootPath",
            errors);

        RequirePositiveInt(
            configuration["Outbox:BatchSize"],
            "Outbox:BatchSize",
            errors);

        RequirePositiveInt(
            configuration["Outbox:PollIntervalSeconds"],
            "Outbox:PollIntervalSeconds",
            errors);

        RequirePositiveInt(
            configuration["Outbox:MaxAttempts"],
            "Outbox:MaxAttempts",
            errors);

        RequirePositiveInt(
            configuration["BookingExports:PollIntervalSeconds"],
            "BookingExports:PollIntervalSeconds",
            errors);

        RequirePositiveInt(
            configuration["BookingExports:MaxAttempts"],
            "BookingExports:MaxAttempts",
            errors);

        RequirePositiveInt(
            configuration["BookingExports:ClaimTimeoutMinutes"],
            "BookingExports:ClaimTimeoutMinutes",
            errors);

        ValidateOpenTelemetry(
            configuration,
            errors);

        if (errors.Count == 0)
        {
            return;
        }

        throw new InvalidOperationException(
            "Invalid BookingHub.Worker startup configuration:" +
            Environment.NewLine +
            string.Join(
                Environment.NewLine,
                errors.Select(
                    error => $"- {error}")));
    }

    private static void ValidateOpenTelemetry(
        IConfiguration configuration,
        ICollection<string> errors)
    {
        if (!configuration.GetValue<bool>(
                "OpenTelemetry:Enabled"))
        {
            return;
        }

        var endpoint =
            configuration["OpenTelemetry:OtlpEndpoint"];

        if (!Uri.TryCreate(
                endpoint,
                UriKind.Absolute,
                out _))
        {
            errors.Add(
                "OpenTelemetry:OtlpEndpoint must be an absolute URI when telemetry is enabled.");
        }
    }

    private static void Require(
        string? value,
        string key,
        ICollection<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add(
                $"{key} is required.");
        }
    }

    private static void RequirePositiveInt(
        string? value,
        string key,
        ICollection<string> errors)
    {
        if (!int.TryParse(
                value,
                out var parsed) ||
            parsed <= 0)
        {
            errors.Add(
                $"{key} must be a positive integer.");
        }
    }
}
