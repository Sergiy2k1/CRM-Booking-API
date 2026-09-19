using BookingHub.Domain.Services;
using Xunit;

namespace BookingHub.Domain.UnitTests.Services;

public sealed class ServiceTests
{
    [Fact]
    public void CreateWithValidDataShouldCreateActiveService()
    {
        var organizationId = Guid.NewGuid();

        var createdAtUtc =
            new DateTimeOffset(
                2026,
                9,
                19,
                10,
                0,
                0,
                TimeSpan.Zero);

        var service = Service.Create(
            Guid.NewGuid(),
            organizationId,
            "Haircut",
            "Classic haircut",
            TimeSpan.FromMinutes(45),
            700m,
            "uah",
            createdAtUtc);

        Assert.Equal(organizationId, service.OrganizationId);
        Assert.Equal("Haircut", service.Name);
        Assert.Equal("Classic haircut", service.Description);
        Assert.Equal(TimeSpan.FromMinutes(45), service.Duration);
        Assert.Equal(700m, service.PriceAmount);
        Assert.Equal("UAH", service.Currency);
        Assert.Equal(ServiceStatus.Active, service.Status);
        Assert.Equal(createdAtUtc, service.CreatedAtUtc);
        Assert.Equal(createdAtUtc, service.UpdatedAtUtc);
    }

    [Fact]
    public void CreateWithZeroDurationShouldThrowArgumentOutOfRangeException()
    {
        var action = () => Service.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Haircut",
            null,
            TimeSpan.Zero,
            700m,
            "UAH",
            DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentOutOfRangeException>(
            action);
    }

    [Fact]
    public void CreateWithNegativePriceShouldThrowArgumentOutOfRangeException()
    {
        var action = () => Service.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Haircut",
            null,
            TimeSpan.FromMinutes(45),
            -1m,
            "UAH",
            DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentOutOfRangeException>(
            action);
    }

    [Fact]
    public void CreateWithInvalidCurrencyShouldThrowArgumentException()
    {
        var action = () => Service.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Haircut",
            null,
            TimeSpan.FromMinutes(45),
            700m,
            "UA",
            DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void UpdateDetailsShouldChangeServiceData()
    {
        var service = CreateService();
        var updatedAtUtc = DateTimeOffset.UtcNow.AddMinutes(10);

        service.UpdateDetails(
            "Premium Haircut",
            "Haircut and styling",
            TimeSpan.FromMinutes(60),
            1000m,
            "usd",
            updatedAtUtc);

        Assert.Equal("Premium Haircut", service.Name);
        Assert.Equal("Haircut and styling", service.Description);
        Assert.Equal(TimeSpan.FromMinutes(60), service.Duration);
        Assert.Equal(1000m, service.PriceAmount);
        Assert.Equal("USD", service.Currency);
        Assert.Equal(
            updatedAtUtc.ToUniversalTime(),
            service.UpdatedAtUtc);
    }

    [Fact]
    public void DeactivateShouldMakeServiceInactive()
    {
        var service = CreateService();

        service.Deactivate(
            DateTimeOffset.UtcNow.AddMinutes(10));

        Assert.Equal(
            ServiceStatus.Inactive,
            service.Status);
    }

    [Fact]
    public void UpdateInactiveServiceShouldThrowInvalidOperationException()
    {
        var service = CreateService();

        service.Deactivate(
            DateTimeOffset.UtcNow);

        var action = () => service.UpdateDetails(
            "New name",
            null,
            TimeSpan.FromMinutes(30),
            500m,
            "UAH",
            DateTimeOffset.UtcNow.AddMinutes(10));

        Assert.Throws<InvalidOperationException>(
            action);
    }

    private static Service CreateService()
    {
        return Service.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Haircut",
            "Classic haircut",
            TimeSpan.FromMinutes(45),
            700m,
            "UAH",
            DateTimeOffset.UtcNow);
    }
}
