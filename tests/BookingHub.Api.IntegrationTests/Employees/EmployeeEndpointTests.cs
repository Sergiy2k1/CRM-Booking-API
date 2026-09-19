using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BookingHub.Api.Contracts.Employees;
using BookingHub.Application.Abstractions;
using BookingHub.Application.Abstractions.Authentication;
using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Domain.Employees;
using BookingHub.Domain.Organizations;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using Xunit;

namespace BookingHub.Api.IntegrationTests.Employees;

public sealed class EmployeeEndpointTests
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
    public async Task GetWithReceptionistRoleShouldReturnEmployee()
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
                $"/api/organizations/{context.OrganizationId}/employees/{context.EmployeeId}",
                TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body =
            await response.Content.ReadFromJsonAsync<EmployeeResponse>(
                JsonOptions,
                TestContext.Current.CancellationToken);

        Assert.NotNull(body);
        Assert.Equal(context.EmployeeId, body.Id);
        Assert.Equal(EmployeeStatus.Active, body.Status);
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
                $"/api/organizations/{context.OrganizationId}/employees",
                new CreateEmployeeRequest(
                    "New",
                    "Employee",
                    "Barber"),
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
                $"/api/organizations/{context.OrganizationId}/employees",
                new CreateEmployeeRequest(
                    "New",
                    "Employee",
                    "Barber"),
                TestContext.Current.CancellationToken);

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);
    }

    [Fact]
    public async Task GetWorkingHoursWithReceptionistRoleShouldReturnOk()
    {
        var context = CreateContext();
        var dependencies = ConfigureDependencies(context);

        dependencies.EmployeeScheduleRepository
            .GetWorkingHoursAsync(
                context.OrganizationId,
                context.EmployeeId,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<IReadOnlyCollection<EmployeeWorkingHours>>(
                    [
                        EmployeeWorkingHours.Create(
                            Guid.NewGuid(),
                            context.OrganizationId,
                            context.EmployeeId,
                            DayOfWeek.Monday,
                            new TimeOnly(9, 0),
                            new TimeOnly(17, 0))
                    ]));

        using var application = CreateApplication(dependencies);
        using var client = application.CreateClient();

        AddAuthorizationHeader(
            client,
            CreateAccessToken(
                context.OrganizationId,
                OrganizationRole.Receptionist));

        using var response =
            await client.GetAsync(
                $"/api/organizations/{context.OrganizationId}/employees/{context.EmployeeId}/working-hours",
                TestContext.Current.CancellationToken);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }

    [Fact]
    public async Task PostWorkingHoursWithReceptionistRoleShouldReturnForbidden()
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
                $"/api/organizations/{context.OrganizationId}/employees/{context.EmployeeId}/working-hours",
                new CreateWorkingHoursRequest(
                    DayOfWeek.Monday,
                    new TimeOnly(9, 0),
                    new TimeOnly(17, 0)),
                TestContext.Current.CancellationToken);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    [Fact]
    public async Task PostWorkingHoursWithManagerRoleShouldReturnCreated()
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
                $"/api/organizations/{context.OrganizationId}/employees/{context.EmployeeId}/working-hours",
                new CreateWorkingHoursRequest(
                    DayOfWeek.Monday,
                    new TimeOnly(9, 0),
                    new TimeOnly(17, 0)),
                TestContext.Current.CancellationToken);

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);
    }

    [Fact]
    public async Task PostTimeOffWithManagerRoleShouldReturnCreated()
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
                $"/api/organizations/{context.OrganizationId}/employees/{context.EmployeeId}/time-off",
                new CreateTimeOffRequest(
                    context.UtcNow.AddDays(1),
                    context.UtcNow.AddDays(1).AddHours(8),
                    "Vacation"),
                TestContext.Current.CancellationToken);

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        var body =
            await response.Content.ReadFromJsonAsync<TimeOffResponse>(
                JsonOptions,
                TestContext.Current.CancellationToken);

        Assert.NotNull(body);
        Assert.Equal(
            EmployeeTimeOffStatus.Active,
            body.Status);
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
                                dependencies.EmployeeRepository);

                            Replace(
                                services,
                                dependencies.EmployeeScheduleRepository);

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

        var employeeRepository =
            Substitute.For<IEmployeeRepository>();

        var employeeScheduleRepository =
            Substitute.For<IEmployeeScheduleRepository>();

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

        employeeRepository
            .GetByOrganizationAndIdAsync(
                context.OrganizationId,
                context.EmployeeId,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<Employee?>(
                    context.Employee));

        employeeRepository
            .GetTrackedByOrganizationAndIdAsync(
                context.OrganizationId,
                context.EmployeeId,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<Employee?>(
                    context.Employee));

        employeeRepository
            .AddAsync(
                Arg.Any<Employee>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        employeeScheduleRepository
            .GetWorkingHoursAsync(
                context.OrganizationId,
                context.EmployeeId,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<IReadOnlyCollection<EmployeeWorkingHours>>(
                    []));

        employeeScheduleRepository
            .HasWorkingHoursOverlapAsync(
                Arg.Any<Guid>(),
                Arg.Any<Guid>(),
                Arg.Any<DayOfWeek>(),
                Arg.Any<TimeOnly>(),
                Arg.Any<TimeOnly>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        employeeScheduleRepository
            .AddWorkingHoursAsync(
                Arg.Any<EmployeeWorkingHours>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        employeeScheduleRepository
            .AddTimeOffAsync(
                Arg.Any<EmployeeTimeOff>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        unitOfWork
            .SaveChangesAsync(
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        clock.UtcNow.Returns(context.UtcNow);

        guidGenerator.NewGuid()
            .Returns(context.GeneratedId);

        return new TestDependencies(
            organizationRepository,
            employeeRepository,
            employeeScheduleRepository,
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
        var employeeId = Guid.NewGuid();
        var generatedId = Guid.NewGuid();

        var utcNow =
            new DateTimeOffset(
                2026,
                9,
                19,
                18,
                0,
                0,
                TimeSpan.Zero);

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
            employeeId,
            generatedId,
            utcNow,
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
        Guid EmployeeId,
        Guid GeneratedId,
        DateTimeOffset UtcNow,
        Employee Employee);

    private sealed record TestDependencies(
        IOrganizationRepository OrganizationRepository,
        IEmployeeRepository EmployeeRepository,
        IEmployeeScheduleRepository EmployeeScheduleRepository,
        IUnitOfWork UnitOfWork,
        IClock Clock,
        IGuidGenerator GuidGenerator);
}
