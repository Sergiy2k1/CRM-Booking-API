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
using BookingHub.Domain.Employees;
using BookingHub.Domain.Organizations;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using Xunit;

namespace BookingHub.Api.IntegrationTests.Bookings;

public sealed class BookingLifecycleEndpointTests
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
    public async Task GetWithReceptionistRoleShouldReturnBooking()
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
                $"/api/organizations/{context.OrganizationId}/bookings/{context.BookingId}",
                TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body =
            await response.Content.ReadFromJsonAsync<BookingResponse>(
                JsonOptions,
                TestContext.Current.CancellationToken);

        Assert.NotNull(body);
        Assert.Equal(context.BookingId, body.Id);
        Assert.Equal(BookingStatus.Pending, body.Status);
    }

    [Fact]
    public async Task ConfirmWithReceptionistRoleShouldReturnConfirmedBooking()
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
                $"/api/organizations/{context.OrganizationId}/bookings/{context.BookingId}/confirm",
                null,
                TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body =
            await response.Content.ReadFromJsonAsync<BookingResponse>(
                JsonOptions,
                TestContext.Current.CancellationToken);

        Assert.NotNull(body);
        Assert.Equal(BookingStatus.Confirmed, body.Status);
    }

    [Fact]
    public async Task GetWithEmployeeRoleShouldReturnForbidden()
    {
        var context = CreateContext();
        var dependencies = ConfigureDependencies(context);

        using var application = CreateApplication(dependencies);
        using var client = application.CreateClient();

        AddAuthorizationHeader(
            client,
            CreateAccessToken(
                context.OrganizationId,
                OrganizationRole.Employee));

        using var response =
            await client.GetAsync(
                $"/api/organizations/{context.OrganizationId}/bookings/{context.BookingId}",
                TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task RescheduleWithManagerRoleShouldReturnUpdatedTime()
    {
        var context = CreateContext();
        var dependencies = ConfigureDependencies(context);

        var newStart =
            context.Booking.StartsAtUtc.AddDays(7);

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
                            newStart.DayOfWeek,
                            new TimeOnly(9, 0),
                            new TimeOnly(18, 0))
                    ]));

        dependencies.BookingRepository
            .GetOverlappingAsync(
                context.OrganizationId,
                context.EmployeeId,
                newStart,
                newStart.AddHours(1),
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<IReadOnlyCollection<Booking>>(
                    [context.Booking]));

        using var application = CreateApplication(dependencies);
        using var client = application.CreateClient();

        AddAuthorizationHeader(
            client,
            CreateAccessToken(
                context.OrganizationId,
                OrganizationRole.Manager));

        using var response =
            await client.PostAsJsonAsync(
                $"/api/organizations/{context.OrganizationId}/bookings/{context.BookingId}/reschedule",
                new RescheduleBookingRequest(newStart),
                TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body =
            await response.Content.ReadFromJsonAsync<BookingResponse>(
                JsonOptions,
                TestContext.Current.CancellationToken);

        Assert.NotNull(body);
        Assert.Equal(newStart, body.StartsAtUtc);
        Assert.Equal(newStart.AddHours(1), body.EndsAtUtc);
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
                                dependencies.BookingRepository);

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
                        }));
    }

    private static TestDependencies ConfigureDependencies(
        ApiTestContext context)
    {
        var bookingRepository =
            Substitute.For<IBookingRepository>();

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

        var organization =
            Organization.Create(
                context.OrganizationId,
                "Beauty Studio",
                "beauty-studio",
                "UTC",
                context.UtcNow);

        var employee =
            Employee.Create(
                context.EmployeeId,
                context.OrganizationId,
                null,
                "Sergiy",
                "Tester",
                "Barber",
                context.UtcNow.AddDays(-1));

        bookingRepository
            .GetByOrganizationAndIdAsync(
                context.OrganizationId,
                context.BookingId,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<Booking?>(
                    context.Booking));

        bookingRepository
            .GetTrackedByOrganizationAndIdAsync(
                context.OrganizationId,
                context.BookingId,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<Booking?>(
                    context.Booking));

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
                    employee));

        employeeScheduleRepository
            .GetTimeOffAsync(
                Arg.Any<Guid>(),
                Arg.Any<Guid>(),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<IReadOnlyCollection<EmployeeTimeOff>>(
                    []));

        unitOfWork
            .SaveChangesAsync(
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        clock.UtcNow.Returns(context.UtcNow);

        return new TestDependencies(
            bookingRepository,
            organizationRepository,
            employeeRepository,
            employeeScheduleRepository,
            unitOfWork,
            clock);
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
        var bookingId = Guid.NewGuid();

        var utcNow =
            new DateTimeOffset(
                2026,
                9,
                19,
                20,
                0,
                0,
                TimeSpan.Zero);

        var startsAtUtc =
            new DateTimeOffset(
                2026,
                9,
                21,
                10,
                0,
                0,
                TimeSpan.Zero);

        var booking =
            Booking.Create(
                bookingId,
                organizationId,
                Guid.NewGuid(),
                employeeId,
                Guid.NewGuid(),
                startsAtUtc,
                startsAtUtc.AddHours(1),
                700m,
                "UAH",
                null,
                utcNow);

        return new ApiTestContext(
            organizationId,
            employeeId,
            bookingId,
            utcNow,
            booking);
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
        Guid BookingId,
        DateTimeOffset UtcNow,
        Booking Booking);

    private sealed record TestDependencies(
        IBookingRepository BookingRepository,
        IOrganizationRepository OrganizationRepository,
        IEmployeeRepository EmployeeRepository,
        IEmployeeScheduleRepository EmployeeScheduleRepository,
        IUnitOfWork UnitOfWork,
        IClock Clock);
}
