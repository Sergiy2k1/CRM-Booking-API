using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using BookingHub.Api.Contracts.Customers;
using BookingHub.Application.Abstractions;
using BookingHub.Application.Abstractions.Authentication;
using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Domain.Customers;
using BookingHub.Domain.Organizations;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using Xunit;

namespace BookingHub.Api.IntegrationTests.Customers;

public sealed class CustomerEndpointTests
{
    private const string SigningKey =
        "development-only-signing-key-change-before-production-2026";

    [Fact]
    public async Task GetWithEmployeeRoleShouldReturnCustomer()
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
                $"/api/organizations/{context.OrganizationId}/customers/{context.CustomerId}",
                TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body =
            await response.Content.ReadFromJsonAsync<CustomerResponse>(
                cancellationToken:
                    TestContext.Current.CancellationToken);

        Assert.NotNull(body);
        Assert.Equal(context.CustomerId, body.Id);
    }

    [Fact]
    public async Task PostWithEmployeeRoleShouldReturnForbidden()
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
            await client.PostAsJsonAsync(
                $"/api/organizations/{context.OrganizationId}/customers",
                new CreateCustomerRequest(
                    "New",
                    "Customer",
                    null,
                    null),
                TestContext.Current.CancellationToken);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    [Fact]
    public async Task PostWithReceptionistRoleShouldReturnCreated()
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
                $"/api/organizations/{context.OrganizationId}/customers",
                new CreateCustomerRequest(
                    "New",
                    "Customer",
                    "new@example.com",
                    null),
                TestContext.Current.CancellationToken);

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);
    }

    [Fact]
    public async Task PutWithManagerRoleShouldReturnUpdatedCustomer()
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
            await client.PutAsJsonAsync(
                $"/api/organizations/{context.OrganizationId}/customers/{context.CustomerId}",
                new UpdateCustomerRequest(
                    "Updated",
                    "Customer",
                    "updated@example.com",
                    null),
                TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body =
            await response.Content.ReadFromJsonAsync<CustomerResponse>(
                cancellationToken:
                    TestContext.Current.CancellationToken);

        Assert.NotNull(body);
        Assert.Equal("Updated", body.FirstName);
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
                                dependencies.CustomerRepository);

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

        var customerRepository =
            Substitute.For<ICustomerRepository>();

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

        customerRepository
            .GetByOrganizationAndIdAsync(
                context.OrganizationId,
                context.CustomerId,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<Customer?>(
                    context.Customer));

        customerRepository
            .GetTrackedByOrganizationAndIdAsync(
                context.OrganizationId,
                context.CustomerId,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<Customer?>(
                    context.Customer));

        customerRepository
            .AddAsync(
                Arg.Any<Customer>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        unitOfWork
            .SaveChangesAsync(
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        clock.UtcNow.Returns(context.UtcNow);
        guidGenerator.NewGuid().Returns(Guid.NewGuid());

        return new TestDependencies(
            organizationRepository,
            customerRepository,
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
        var customerId = Guid.NewGuid();

        var utcNow =
            new DateTimeOffset(
                2026,
                9,
                19,
                17,
                0,
                0,
                TimeSpan.Zero);

        var customer =
            Customer.Create(
                customerId,
                organizationId,
                "Sergiy",
                "Tester",
                "sergiy@example.com",
                "+380501234567",
                utcNow.AddDays(-1));

        return new ApiTestContext(
            organizationId,
            customerId,
            utcNow,
            customer);
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
        Guid CustomerId,
        DateTimeOffset UtcNow,
        Customer Customer);

    private sealed record TestDependencies(
        IOrganizationRepository OrganizationRepository,
        ICustomerRepository CustomerRepository,
        IUnitOfWork UnitOfWork,
        IClock Clock,
        IGuidGenerator GuidGenerator);
}
