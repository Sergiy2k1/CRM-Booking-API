using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BookingHub.Api.Contracts.Bookings;
using BookingHub.Application.Abstractions;
using BookingHub.Application.Abstractions.Authentication;
using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Domain.Bookings;
using BookingHub.Domain.Customers;
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

namespace BookingHub.Api.IntegrationTests.Bookings;

public sealed class CreateBookingEndpointTests
{
    private const string SigningKey =
        "development-only-signing-key-change-before-production-2026";

    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web)
        {
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };

    [Fact]
    public async Task PostWithValidTenantTokenShouldReturnCreatedBooking()
    {
        var context = CreateContext();
        var dependencies = ConfigureDependencies(context);

        using var application = CreateApplication(dependencies);
        using var client = application.CreateClient();

        AddAuthorizationHeader(
            client,
            CreateAccessToken(context.OrganizationId));

        var request = CreateRequest(context);

        using var response =
            await client.PostAsJsonAsync(
                $"/api/organizations/{context.OrganizationId}/bookings",
                request,
                TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body =
            await response.Content.ReadFromJsonAsync<CreateBookingResponse>(
                JsonOptions,
                TestContext.Current.CancellationToken);

        Assert.NotNull(body);
        Assert.Equal(context.BookingId, body.BookingId);
        Assert.Equal(BookingStatus.Pending, body.Status);
    }

    [Fact]
    public async Task PostWithoutAccessTokenShouldReturnUnauthorized()
    {
        var context = CreateContext();
        var dependencies = ConfigureDependencies(context);

        using var application = CreateApplication(dependencies);
        using var client = application.CreateClient();

        using var response =
            await client.PostAsJsonAsync(
                $"/api/organizations/{context.OrganizationId}/bookings",
                CreateRequest(context),
                TestContext.Current.CancellationToken);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task PostWithDifferentTenantTokenShouldReturnForbidden()
    {
        var context = CreateContext();
        var dependencies = ConfigureDependencies(context);

        using var application = CreateApplication(dependencies);
        using var client = application.CreateClient();

        AddAuthorizationHeader(
            client,
            CreateAccessToken(Guid.NewGuid()));

        using var response =
            await client.PostAsJsonAsync(
                $"/api/organizations/{context.OrganizationId}/bookings",
                CreateRequest(context),
                TestContext.Current.CancellationToken);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    [Fact]
    public async Task PostWithBookingConflictShouldReturnConflictProblem()
    {
        var context = CreateContext();
        var dependencies = ConfigureDependencies(context);

        var conflictingBooking =
            Booking.Create(
                Guid.NewGuid(),
                context.OrganizationId,
                Guid.NewGuid(),
                context.EmployeeId,
                Guid.NewGuid(),
                context.StartsAtUtc.AddMinutes(30),
                context.StartsAtUtc.AddMinutes(90),
                500m,
                "UAH",
                null,
                context.CreatedAtUtc);

        dependencies.BookingRepository
            .GetOverlappingAsync(
                context.OrganizationId,
                context.EmployeeId,
                context.StartsAtUtc,
                context.StartsAtUtc.AddHours(1),
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<IReadOnlyCollection<Booking>>(
                    [conflictingBooking]));

        using var application = CreateApplication(dependencies);
        using var client = application.CreateClient();

        AddAuthorizationHeader(
            client,
            CreateAccessToken(context.OrganizationId));

        using var response =
            await client.PostAsJsonAsync(
                $"/api/organizations/{context.OrganizationId}/bookings",
                CreateRequest(context),
                TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var problem =
            await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>(
                JsonOptions,
                TestContext.Current.CancellationToken);

        Assert.NotNull(problem);
        Assert.Equal("Booking slot unavailable", problem.Title);
        Assert.Equal("BookingConflict", problem.AvailabilityStatus);
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
        Guid organizationId)
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
                OrganizationRole.Admin.ToString())
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

    private static CreateBookingRequest CreateRequest(
        ApiTestContext context)
    {
        return new CreateBookingRequest(
            context.CustomerId,
            context.EmployeeId,
            context.ServiceId,
            context.StartsAtUtc,
            "First visit");
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
                            Replace(services, dependencies.OrganizationRepository);
                            Replace(services, dependencies.CustomerRepository);
                            Replace(services, dependencies.EmployeeRepository);
                            Replace(services, dependencies.ServiceRepository);
                            Replace(services, dependencies.EmployeeServiceRepository);
                            Replace(services, dependencies.EmployeeScheduleRepository);
                            Replace(services, dependencies.BookingRepository);
                            Replace(services, dependencies.UnitOfWork);
                            Replace(services, dependencies.Clock);
                            Replace(services, dependencies.GuidGenerator);
                        }));
    }

    private static TestDependencies ConfigureDependencies(
        ApiTestContext context)
    {
        var organizationRepository = Substitute.For<IOrganizationRepository>();
        var customerRepository = Substitute.For<ICustomerRepository>();
        var employeeRepository = Substitute.For<IEmployeeRepository>();
        var serviceRepository = Substitute.For<IServiceRepository>();
        var employeeServiceRepository = Substitute.For<IEmployeeServiceRepository>();
        var employeeScheduleRepository = Substitute.For<IEmployeeScheduleRepository>();
        var bookingRepository = Substitute.For<IBookingRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var clock = Substitute.For<IClock>();
        var guidGenerator = Substitute.For<IGuidGenerator>();

        var organization =
            Organization.Create(
                context.OrganizationId,
                "Beauty Studio",
                "beauty-studio",
                "UTC",
                context.CreatedAtUtc);

        var customer =
            Customer.Create(
                context.CustomerId,
                context.OrganizationId,
                "Sergiy",
                "Tester",
                "sergiy@example.com",
                "+380501234567",
                context.CreatedAtUtc);

        var employee =
            Employee.Create(
                context.EmployeeId,
                context.OrganizationId,
                null,
                "Sergiy",
                "Tester",
                "Barber",
                context.CreatedAtUtc);

        var service =
            Service.Create(
                context.ServiceId,
                context.OrganizationId,
                "Haircut",
                "Classic haircut",
                TimeSpan.FromHours(1),
                700m,
                "UAH",
                context.CreatedAtUtc);

        var workingHours =
            EmployeeWorkingHours.Create(
                Guid.NewGuid(),
                context.OrganizationId,
                context.EmployeeId,
                DayOfWeek.Monday,
                new TimeOnly(9, 0),
                new TimeOnly(18, 0));

        organizationRepository
            .GetByIdAsync(
                context.OrganizationId,
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Organization?>(organization));

        customerRepository
            .GetByIdAsync(
                context.CustomerId,
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Customer?>(customer));

        employeeRepository
            .GetByIdAsync(
                context.EmployeeId,
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Employee?>(employee));

        serviceRepository
            .GetByIdAsync(
                context.ServiceId,
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Service?>(service));

        employeeServiceRepository
            .IsAssignedAsync(
                context.OrganizationId,
                context.EmployeeId,
                context.ServiceId,
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        employeeScheduleRepository
            .GetWorkingHoursAsync(
                context.OrganizationId,
                context.EmployeeId,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<IReadOnlyCollection<EmployeeWorkingHours>>(
                    [workingHours]));

        employeeScheduleRepository
            .GetTimeOffAsync(
                context.OrganizationId,
                context.EmployeeId,
                context.StartsAtUtc,
                context.StartsAtUtc.AddHours(1),
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<IReadOnlyCollection<EmployeeTimeOff>>([]));

        bookingRepository
            .GetOverlappingAsync(
                context.OrganizationId,
                context.EmployeeId,
                context.StartsAtUtc,
                context.StartsAtUtc.AddHours(1),
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<IReadOnlyCollection<Booking>>([]));

        bookingRepository
            .AddAsync(
                Arg.Any<Booking>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        unitOfWork
            .SaveChangesAsync(
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        clock.UtcNow.Returns(context.CreatedAtUtc);
        guidGenerator.NewGuid().Returns(context.BookingId);

        return new TestDependencies(
            organizationRepository,
            customerRepository,
            employeeRepository,
            serviceRepository,
            employeeServiceRepository,
            employeeScheduleRepository,
            bookingRepository,
            unitOfWork,
            clock,
            guidGenerator);
    }

    private static void Replace<TService>(
        IServiceCollection services,
        TService implementation)
        where TService : class
    {
        services.RemoveAll<TService>();
        services.AddSingleton(implementation);
    }

    private static ApiTestContext CreateContext()
    {
        return new ApiTestContext(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateTimeOffset(
                2026,
                9,
                21,
                10,
                0,
                0,
                TimeSpan.Zero),
            new DateTimeOffset(
                2026,
                9,
                20,
                12,
                0,
                0,
                TimeSpan.Zero));
    }

    private sealed record ApiTestContext(
        Guid OrganizationId,
        Guid CustomerId,
        Guid EmployeeId,
        Guid ServiceId,
        Guid BookingId,
        DateTimeOffset StartsAtUtc,
        DateTimeOffset CreatedAtUtc);

    private sealed record TestDependencies(
        IOrganizationRepository OrganizationRepository,
        ICustomerRepository CustomerRepository,
        IEmployeeRepository EmployeeRepository,
        IServiceRepository ServiceRepository,
        IEmployeeServiceRepository EmployeeServiceRepository,
        IEmployeeScheduleRepository EmployeeScheduleRepository,
        IBookingRepository BookingRepository,
        IUnitOfWork UnitOfWork,
        IClock Clock,
        IGuidGenerator GuidGenerator);

    private sealed record ProblemDetailsResponse(
        string? Title,
        string? Detail,
        string? AvailabilityStatus);
}
