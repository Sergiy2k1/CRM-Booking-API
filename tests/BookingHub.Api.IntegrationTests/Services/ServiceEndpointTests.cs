using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BookingHub.Api.Contracts.Services;
using BookingHub.Application.Abstractions;
using BookingHub.Application.Abstractions.Authentication;
using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Domain.Employees;
using BookingHub.Domain.Organizations;
using BookingHub.Domain.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using Xunit;

namespace BookingHub.Api.IntegrationTests.Services;

public sealed class ServiceEndpointTests
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web)
        {
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };

    private const string SigningKey =
        "development-only-signing-key-change-before-production-2026";

    [Fact]
    public async Task GetWithReceptionistRoleShouldReturnService()
    {
        var context = CreateContext();
        var dependencies = ConfigureDependencies(context);

        using var application = CreateApplication(dependencies);
        using var client = application.CreateClient();

        AddAuthorizationHeader(
            client,
            CreateAccessToken(
                context.OrganizationId,
                OrganizationRole.Receptionist));

        using var response =
            await client.GetAsync(
                $"/api/organizations/{context.OrganizationId}/services/{context.ServiceId}",
                TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body =
            await response.Content.ReadFromJsonAsync<ServiceResponse>(
                JsonOptions,
                TestContext.Current.CancellationToken);

        Assert.NotNull(body);
        Assert.Equal(context.ServiceId, body.Id);
        Assert.Equal(ServiceStatus.Active, body.Status);
    }

    [Fact]
    public async Task PostWithReceptionistRoleShouldReturnForbidden()
    {
        var context = CreateContext();
        var dependencies = ConfigureDependencies(context);

        using var application = CreateApplication(dependencies);
        using var client = application.CreateClient();

        AddAuthorizationHeader(
            client,
            CreateAccessToken(
                context.OrganizationId,
                OrganizationRole.Receptionist));

        using var response =
            await client.PostAsJsonAsync(
                $"/api/organizations/{context.OrganizationId}/services",
                new CreateServiceRequest(
                    "Haircut",
                    "Classic",
                    TimeSpan.FromHours(1),
                    700m,
                    "UAH"),
                TestContext.Current.CancellationToken);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    [Fact]
    public async Task PostWithManagerRoleShouldReturnCreated()
    {
        var context = CreateContext();
        var dependencies = ConfigureDependencies(context);

        using var application = CreateApplication(dependencies);
        using var client = application.CreateClient();

        AddAuthorizationHeader(
            client,
            CreateAccessToken(
                context.OrganizationId,
                OrganizationRole.Manager));

        using var response =
            await client.PostAsJsonAsync(
                $"/api/organizations/{context.OrganizationId}/services",
                new CreateServiceRequest(
                    "Premium Haircut",
                    "Premium",
                    TimeSpan.FromHours(1),
                    900m,
                    "UAH"),
                TestContext.Current.CancellationToken);

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);
    }

    [Fact]
    public async Task AssignWithManagerRoleShouldReturnCreated()
    {
        var context = CreateContext();
        var dependencies = ConfigureDependencies(context);

        using var application = CreateApplication(dependencies);
        using var client = application.CreateClient();

        AddAuthorizationHeader(
            client,
            CreateAccessToken(
                context.OrganizationId,
                OrganizationRole.Manager));

        using var response =
            await client.PostAsync(
                $"/api/organizations/{context.OrganizationId}/employees/{context.EmployeeId}/services/{context.ServiceId}",
                null,
                TestContext.Current.CancellationToken);

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);
    }

    [Fact]
    public async Task AssignWithReceptionistRoleShouldReturnForbidden()
    {
        var context = CreateContext();
        var dependencies = ConfigureDependencies(context);

        using var application = CreateApplication(dependencies);
        using var client = application.CreateClient();

        AddAuthorizationHeader(
            client,
            CreateAccessToken(
                context.OrganizationId,
                OrganizationRole.Receptionist));

        using var response =
            await client.PostAsync(
                $"/api/organizations/{context.OrganizationId}/employees/{context.EmployeeId}/services/{context.ServiceId}",
                null,
                TestContext.Current.CancellationToken);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    private static WebApplicationFactory<Program> CreateApplication(
        TestDependencies dependencies)
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(
                builder =>
                    builder.ConfigureTestServices(
                        services =>
                        {
                            Replace(
                                services,
                                dependencies.OrganizationRepository);

                            Replace(
                                services,
                                dependencies.ServiceRepository);

                            Replace(
                                services,
                                dependencies.EmployeeRepository);

                            Replace(
                                services,
                                dependencies.EmployeeServiceRepository);

                            Replace(
                                services,
                                dependencies.UnitOfWork);

                            Replace(
                                services,
                                dependencies.Clock);

                            Replace(
                                services,
                                dependencies.GuidGenerator);
                        }));
    }

    private static TestDependencies ConfigureDependencies(
        ApiTestContext context)
    {
        var organizationRepository =
            Substitute.For<IOrganizationRepository>();

        var serviceRepository =
            Substitute.For<IServiceRepository>();

        var employeeRepository =
            Substitute.For<IEmployeeRepository>();

        var employeeServiceRepository =
            Substitute.For<IEmployeeServiceRepository>();

        var unitOfWork =
            Substitute.For<IUnitOfWork>();

        var clock =
            Substitute.For<IClock>();

        var guidGenerator =
            Substitute.For<IGuidGenerator>();

        var organization =
            Organization.Create(
                context.OrganizationId,
                "Beauty Studio",
                "beauty-studio",
                "UTC",
                context.UtcNow);

        organizationRepository
            .GetByIdAsync(
                context.OrganizationId,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<Organization?>(
                    organization));

        serviceRepository
            .GetByOrganizationAndIdAsync(
                context.OrganizationId,
                context.ServiceId,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<Service?>(
                    context.Service));

        serviceRepository
            .GetTrackedByOrganizationAndIdAsync(
                context.OrganizationId,
                context.ServiceId,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<Service?>(
                    context.Service));

        serviceRepository
            .AddAsync(
                Arg.Any<Service>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        employeeRepository
            .GetByOrganizationAndIdAsync(
                context.OrganizationId,
                context.EmployeeId,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<Employee?>(
                    context.Employee));

        employeeServiceRepository
            .IsAssignedAsync(
                context.OrganizationId,
                context.EmployeeId,
                context.ServiceId,
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        employeeServiceRepository
            .AddAsync(
                Arg.Any<EmployeeService>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        unitOfWork
            .SaveChangesAsync(
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        clock.UtcNow.Returns(context.UtcNow);
        guidGenerator.NewGuid().Returns(context.GeneratedId);

        return new TestDependencies(
            organizationRepository,
            serviceRepository,
            employeeRepository,
            employeeServiceRepository,
            unitOfWork,
            clock,
            guidGenerator);
    }

    private static void AddAuthorizationHeader(
        HttpClient client,
        string accessToken)
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);
    }

    private static string CreateAccessToken(
        Guid organizationId,
        OrganizationRole role)
    {
        var now = DateTime.UtcNow;

        var claims = new[]
        {
            new Claim(
                JwtRegisteredClaimNames.Sub,
                Guid.NewGuid().ToString()),
            new Claim(
                AuthenticationClaimTypes.OrganizationId,
                organizationId.ToString()),
            new Claim(
                ClaimTypes.Role,
                role.ToString())
        };

        var credentials =
            new SigningCredentials(
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(SigningKey)),
                SecurityAlgorithms.HmacSha256);

        var token =
            new JwtSecurityToken(
                issuer: "BookingHub",
                audience: "BookingHub.Api",
                claims: claims,
                notBefore: now.AddMinutes(-1),
                expires: now.AddMinutes(15),
                signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }

    private static ApiTestContext CreateContext()
    {
        var organizationId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var generatedId = Guid.NewGuid();

        var utcNow =
            new DateTimeOffset(
                2026,
                9,
                19,
                19,
                0,
                0,
                TimeSpan.Zero);

        var service =
            Service.Create(
                serviceId,
                organizationId,
                "Haircut",
                "Classic",
                TimeSpan.FromHours(1),
                700m,
                "UAH",
                utcNow.AddDays(-1));

        var employee =
            Employee.Create(
                employeeId,
                organizationId,
                null,
                "Sergiy",
                "Tester",
                "Barber",
                utcNow.AddDays(-1));

        return new ApiTestContext(
            organizationId,
            serviceId,
            employeeId,
            generatedId,
            utcNow,
            service,
            employee);
    }

    private static void Replace<TService>(
        IServiceCollection services,
        TService implementation)
        where TService : class
    {
        services.RemoveAll<TService>();
        services.AddSingleton(implementation);
    }

    private sealed record ApiTestContext(
        Guid OrganizationId,
        Guid ServiceId,
        Guid EmployeeId,
        Guid GeneratedId,
        DateTimeOffset UtcNow,
        Service Service,
        Employee Employee);

    private sealed record TestDependencies(
        IOrganizationRepository OrganizationRepository,
        IServiceRepository ServiceRepository,
        IEmployeeRepository EmployeeRepository,
        IEmployeeServiceRepository EmployeeServiceRepository,
        IUnitOfWork UnitOfWork,
        IClock Clock,
        IGuidGenerator GuidGenerator);
}
