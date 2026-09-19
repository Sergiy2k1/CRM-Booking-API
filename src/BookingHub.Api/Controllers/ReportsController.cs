using BookingHub.Api.Authorization;
using BookingHub.Api.Contracts.Reporting;
using BookingHub.Application.Reporting.BookingSummary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingHub.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.Reporting)]
[Route("api/organizations/{organizationId:guid}/reports")]
public sealed class ReportsController : ControllerBase
{
    private readonly GetBookingSummaryReportHandler _bookingSummaryHandler;

    public ReportsController(
        GetBookingSummaryReportHandler bookingSummaryHandler)
    {
        _bookingSummaryHandler = bookingSummaryHandler;
    }

    [HttpGet("bookings/summary")]
    public async Task<ActionResult<BookingSummaryReportResponse>> GetBookingSummary(
        Guid organizationId,
        [FromQuery] DateTimeOffset fromUtc,
        [FromQuery] DateTimeOffset toUtc,
        CancellationToken cancellationToken)
    {
        var report =
            await _bookingSummaryHandler.HandleAsync(
                new GetBookingSummaryReportQuery(
                    organizationId,
                    fromUtc,
                    toUtc),
                cancellationToken);

        return Ok(
            new BookingSummaryReportResponse(
                report.OrganizationId,
                report.FromUtc,
                report.ToUtc,
                report.TotalBookings,
                report.PendingCount,
                report.ConfirmedCount,
                report.CompletedCount,
                report.CancelledCount,
                report.NoShowCount,
                report.CompletedRevenue
                    .Select(
                        x => new BookingRevenueResponse(
                            x.Currency,
                            x.Amount))
                    .ToArray()));
    }
}
