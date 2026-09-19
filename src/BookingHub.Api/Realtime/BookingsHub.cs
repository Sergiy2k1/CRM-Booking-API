using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BookingHub.Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BookingHub.Api.Realtime;

[Authorize]
public sealed class BookingsHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var organizationIdValue =
            Context.User?.FindFirstValue(
                AuthenticationClaimTypes.OrganizationId);

        if (!Guid.TryParse(
                organizationIdValue,
                out var organizationId))
        {
            Context.Abort();
            return;
        }

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            RealtimeGroupNames.Organization(
                organizationId));

        var userIdValue =
            Context.User?.FindFirstValue(
                JwtRegisteredClaimNames.Sub)
            ?? Context.User?.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (Guid.TryParse(
                userIdValue,
                out var userId))
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                RealtimeGroupNames.User(
                    userId));
        }

        await base.OnConnectedAsync();
    }
}
