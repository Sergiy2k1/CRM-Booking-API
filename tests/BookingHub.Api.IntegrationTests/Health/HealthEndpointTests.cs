using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace BookingHub.Api.IntegrationTests.Health;

public sealed class HealthEndpointTests
{
    [Fact]
    public async Task LivenessShouldReturnOk()
    {
        using var application =
            new WebApplicationFactory<Program>();

        using var client =
            application.CreateClient();

        using var response =
            await client.GetAsync(
                "/health/live",
                TestContext.Current.CancellationToken);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }
}
