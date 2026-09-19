using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using BookingHub.Api.Authorization;
using BookingHub.Api.ErrorHandling;
using BookingHub.Application;
using BookingHub.Domain.Organizations;
using BookingHub.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

var jwtIssuer =
    builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException(
        "JWT issuer is not configured.");

var jwtAudience =
    builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException(
        "JWT audience is not configured.");

var jwtSigningKey =
    builder.Configuration["Jwt:SigningKey"]
    ?? throw new InvalidOperationException(
        "JWT signing key is not configured.");

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(
        options =>
        {
            options.TokenValidationParameters =
                new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = jwtAudience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey =
                        new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(
                                jwtSigningKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    RoleClaimType = ClaimTypes.Role
                };
        });

builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton<
    IAuthorizationHandler,
    OrganizationAccessHandler>();

builder.Services.AddAuthorization(
    options =>
    {
        options.AddPolicy(
            AuthorizationPolicies.OrganizationAccess,
            policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(
                    new OrganizationAccessRequirement());
            });

        options.AddPolicy(
            AuthorizationPolicies.BookingManagement,
            policy =>
            {
                policy.RequireAuthenticatedUser();

                policy.RequireRole(
                    OrganizationRole.Owner.ToString(),
                    OrganizationRole.Admin.ToString(),
                    OrganizationRole.Manager.ToString(),
                    OrganizationRole.Receptionist.ToString());

                policy.AddRequirements(
                    new OrganizationAccessRequirement());
            });
    });

builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services
    .AddControllers()
    .AddJsonOptions(
        options =>
            options.JsonSerializerOptions.Converters.Add(
                new JsonStringEnumConverter()));

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();

app.Run();

public partial class Program;
