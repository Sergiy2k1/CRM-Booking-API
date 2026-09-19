namespace BookingHub.Api.Configuration;

internal static class StartupConfigurationValidator
{
    private const int MinimumSigningKeyLength = 32;

    public static void Validate(
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        var errors =
            new List<string>();

        Require(
            configuration.GetConnectionString("Database"),
            "ConnectionStrings:Database",
            errors);

        Require(
            configuration["Jwt:Issuer"],
            "Jwt:Issuer",
            errors);

        Require(
            configuration["Jwt:Audience"],
            "Jwt:Audience",
            errors);

        var signingKey =
            configuration["Jwt:SigningKey"];

        Require(
            signingKey,
            "Jwt:SigningKey",
            errors);

        if (!string.IsNullOrWhiteSpace(signingKey) &&
            signingKey.Length < MinimumSigningKeyLength)
        {
            errors.Add(
                $"Jwt:SigningKey must contain at least {MinimumSigningKeyLength} characters.");
        }

        if (!environment.IsDevelopment() &&
            !string.IsNullOrWhiteSpace(signingKey) &&
            signingKey.StartsWith(
                "development-only",
                StringComparison.OrdinalIgnoreCase))
        {
            errors.Add(
                "Jwt:SigningKey must not use the development placeholder outside Development.");
        }

        RequirePositiveInt(
            configuration["Jwt:AccessTokenMinutes"],
            "Jwt:AccessTokenMinutes",
            errors);

        RequirePositiveInt(
            configuration["Jwt:RefreshTokenDays"],
            "Jwt:RefreshTokenDays",
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
            configuration["RabbitMq:Queue"],
            "RabbitMq:Queue",
            errors);

        Require(
            configuration["ExportStorage:RootPath"],
            "ExportStorage:RootPath",
            errors);

        ValidateOpenTelemetry(
            configuration,
            errors);

        if (errors.Count == 0)
        {
            return;
        }

        throw new InvalidOperationException(
            "Invalid BookingHub.Api startup configuration:" +
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
