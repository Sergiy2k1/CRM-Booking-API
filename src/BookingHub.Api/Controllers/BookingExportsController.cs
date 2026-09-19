using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BookingHub.Api.Authorization;
using BookingHub.Api.Contracts.Reporting;
using BookingHub.Application.Reporting.BookingExports;
using BookingHub.Application.Reporting.BookingExports.DownloadBookingCsvExport;
using BookingHub.Application.Reporting.BookingExports.GetBookingCsvExport;
using BookingHub.Application.Reporting.BookingExports.RequestBookingCsvExport;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingHub.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.Reporting)]
[Route("api/organizations/{organizationId:guid}/reports/bookings/exports")]
public sealed class BookingExportsController : ControllerBase
{
    private readonly RequestBookingCsvExportHandler _requestHandler;
    private readonly GetBookingCsvExportHandler _getHandler;
    private readonly DownloadBookingCsvExportHandler _downloadHandler;

    public BookingExportsController(
        RequestBookingCsvExportHandler requestHandler,
        GetBookingCsvExportHandler getHandler,
        DownloadBookingCsvExportHandler downloadHandler)
    {
        _requestHandler = requestHandler;
        _getHandler = getHandler;
        _downloadHandler = downloadHandler;
    }

    [HttpPost]
    public async Task<ActionResult<BookingExportResponse>> RequestExport(
        Guid organizationId,
        RequestBookingCsvExportRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var result =
            await _requestHandler.HandleAsync(
                new RequestBookingCsvExportCommand(
                    organizationId,
                    userId,
                    request.FromUtc,
                    request.ToUtc),
                cancellationToken);

        return AcceptedAtAction(
            nameof(GetById),
            new
            {
                organizationId,
                exportId = result.Id
            },
            Map(result));
    }

    [HttpGet("{exportId:guid}")]
    public async Task<ActionResult<BookingExportResponse>> GetById(
        Guid organizationId,
        Guid exportId,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var result =
            await _getHandler.HandleAsync(
                new GetBookingCsvExportQuery(
                    organizationId,
                    userId,
                    exportId),
                cancellationToken);

        return Ok(Map(result));
    }

    [HttpGet("{exportId:guid}/download")]
    public async Task<IActionResult> Download(
        Guid organizationId,
        Guid exportId,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var result =
            await _downloadHandler.HandleAsync(
                new DownloadBookingCsvExportQuery(
                    organizationId,
                    userId,
                    exportId),
                cancellationToken);

        return File(
            result.Content,
            "text/csv; charset=utf-8",
            result.FileName);
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

    private static BookingExportResponse Map(
        BookingExportDetails export)
    {
        return new BookingExportResponse(
            export.Id,
            export.FromUtc,
            export.ToUtc,
            export.Status,
            export.CreatedAtUtc,
            export.StartedAtUtc,
            export.CompletedAtUtc,
            export.AttemptCount,
            export.LastError,
            export.FileName);
    }
}
