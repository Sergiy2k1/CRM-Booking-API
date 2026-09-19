using BookingHub.Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace BookingHub.Api.Authorization;

public sealed class OrganizationAccessHandler
    : AuthorizationHandler<OrganizationAccessRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OrganizationAccessHandler(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OrganizationAccessRequirement requirement)
    {
        var httpContext =
            _httpContextAccessor.HttpContext;

        if (httpContext is null)
        {
            return Task.CompletedTask;
        }

        var claimValue =
            context.User.FindFirst(
                AuthenticationClaimTypes.OrganizationId)
                ?.Value;

        var routeValue =
            httpContext.Request.RouteValues["organizationId"]
                ?.ToString();

        if (Guid.TryParse(claimValue, out var claimOrganizationId) &&
            Guid.TryParse(routeValue, out var routeOrganizationId) &&
            claimOrganizationId == routeOrganizationId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
