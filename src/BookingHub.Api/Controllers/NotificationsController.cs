using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BookingHub.Api.Authorization;
using BookingHub.Api.Contracts.Notifications;
using BookingHub.Application.Notifications.Common;
using BookingHub.Application.Notifications.ListNotifications;
using BookingHub.Application.Notifications.MarkNotificationRead;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingHub.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.OrganizationAccess)]
[Route("api/organizations/{organizationId:guid}/notifications")]
public sealed class NotificationsController : ControllerBase
{
    private readonly ListNotificationsHandler _listNotificationsHandler;
    private readonly MarkNotificationReadHandler _markNotificationReadHandler;

    public NotificationsController(
        ListNotificationsHandler listNotificationsHandler,
        MarkNotificationReadHandler markNotificationReadHandler)
    {
        _listNotificationsHandler = listNotificationsHandler;
        _markNotificationReadHandler = markNotificationReadHandler;
    }

    [HttpGet]
    public async Task<ActionResult<NotificationListResponse>> List(
        Guid organizationId,
        [FromQuery] bool? isRead,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var result =
            await _listNotificationsHandler.HandleAsync(
                new ListNotificationsQuery(
                    organizationId,
                    userId,
                    isRead,
                    page,
                    pageSize),
                cancellationToken);

        return Ok(
            new NotificationListResponse(
                result.Items.Select(Map).ToArray(),
                result.Page,
                result.PageSize,
                result.TotalCount));
    }

    [HttpPost("{notificationId:guid}/read")]
    public async Task<ActionResult<NotificationResponse>> MarkRead(
        Guid organizationId,
        Guid notificationId,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var result =
            await _markNotificationReadHandler.HandleAsync(
                new MarkNotificationReadCommand(
                    organizationId,
                    userId,
                    notificationId),
                cancellationToken);

        return Ok(Map(result));
    }

    private bool TryGetUserId(
        out Guid userId)
    {
        var userIdValue =
            User.FindFirstValue(
                JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        return Guid.TryParse(
            userIdValue,
            out userId);
    }

    private static NotificationResponse Map(
        NotificationDetails notification)
    {
        return new NotificationResponse(
            notification.Id,
            notification.Type,
            notification.Title,
            notification.Message,
            notification.RelatedBookingId,
            notification.CreatedAtUtc,
            notification.IsRead,
            notification.ReadAtUtc);
    }
}
