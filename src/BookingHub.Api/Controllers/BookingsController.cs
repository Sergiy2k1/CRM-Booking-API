using BookingHub.Api.Authorization;
using BookingHub.Api.Contracts.Bookings;
using BookingHub.Application.Bookings.ChangeBookingStatus;
using BookingHub.Application.Bookings.Common;
using BookingHub.Application.Bookings.CreateBooking;
using BookingHub.Application.Bookings.GetBooking;
using BookingHub.Application.Bookings.ListBookings;
using BookingHub.Application.Bookings.RescheduleBooking;
using BookingHub.Domain.Bookings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingHub.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.BookingManagement)]
[Route("api/organizations/{organizationId:guid}/bookings")]
public sealed class BookingsController : ControllerBase
{
    private readonly CreateBookingHandler _createBookingHandler;
    private readonly GetBookingHandler _getBookingHandler;
    private readonly ListBookingsHandler _listBookingsHandler;
    private readonly ChangeBookingStatusHandler _changeBookingStatusHandler;
    private readonly RescheduleBookingHandler _rescheduleBookingHandler;

    public BookingsController(
        CreateBookingHandler createBookingHandler,
        GetBookingHandler getBookingHandler,
        ListBookingsHandler listBookingsHandler,
        ChangeBookingStatusHandler changeBookingStatusHandler,
        RescheduleBookingHandler rescheduleBookingHandler)
    {
        _createBookingHandler = createBookingHandler;
        _getBookingHandler = getBookingHandler;
        _listBookingsHandler = listBookingsHandler;
        _changeBookingStatusHandler = changeBookingStatusHandler;
        _rescheduleBookingHandler = rescheduleBookingHandler;
    }

    [HttpPost]
    public async Task<ActionResult<CreateBookingResponse>> Create(
        Guid organizationId,
        CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await _createBookingHandler.HandleAsync(
                new CreateBookingCommand(
                    organizationId,
                    request.CustomerId,
                    request.EmployeeId,
                    request.ServiceId,
                    request.StartsAtUtc,
                    request.Notes),
                cancellationToken);

        return Created(
            $"/api/organizations/{organizationId}/bookings/{result.BookingId}",
            new CreateBookingResponse(
                result.BookingId,
                result.Status,
                result.StartsAtUtc,
                result.EndsAtUtc,
                result.PriceAmount,
                result.Currency));
    }

    [HttpGet("{bookingId:guid}")]
    public async Task<ActionResult<BookingResponse>> GetById(
        Guid organizationId,
        Guid bookingId,
        CancellationToken cancellationToken)
    {
        var result =
            await _getBookingHandler.HandleAsync(
                new GetBookingQuery(
                    organizationId,
                    bookingId),
                cancellationToken);

        return Ok(Map(result));
    }

    [HttpGet]
    public async Task<ActionResult<BookingListResponse>> List(
        Guid organizationId,
        [FromQuery] BookingStatus? status,
        [FromQuery] Guid? employeeId,
        [FromQuery] DateTimeOffset? startsFromUtc,
        [FromQuery] DateTimeOffset? startsBeforeUtc,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result =
            await _listBookingsHandler.HandleAsync(
                new ListBookingsQuery(
                    organizationId,
                    status,
                    employeeId,
                    startsFromUtc,
                    startsBeforeUtc,
                    page,
                    pageSize),
                cancellationToken);

        return Ok(
            new BookingListResponse(
                result.Items.Select(Map).ToArray(),
                result.Page,
                result.PageSize,
                result.TotalCount));
    }

    [HttpPost("{bookingId:guid}/confirm")]
    public Task<ActionResult<BookingResponse>> Confirm(
        Guid organizationId,
        Guid bookingId,
        CancellationToken cancellationToken)
    {
        return ChangeStatus(
            organizationId,
            bookingId,
            BookingTransition.Confirm,
            cancellationToken);
    }

    [HttpPost("{bookingId:guid}/cancel")]
    public Task<ActionResult<BookingResponse>> Cancel(
        Guid organizationId,
        Guid bookingId,
        CancellationToken cancellationToken)
    {
        return ChangeStatus(
            organizationId,
            bookingId,
            BookingTransition.Cancel,
            cancellationToken);
    }

    [HttpPost("{bookingId:guid}/complete")]
    public Task<ActionResult<BookingResponse>> Complete(
        Guid organizationId,
        Guid bookingId,
        CancellationToken cancellationToken)
    {
        return ChangeStatus(
            organizationId,
            bookingId,
            BookingTransition.Complete,
            cancellationToken);
    }

    [HttpPost("{bookingId:guid}/no-show")]
    public Task<ActionResult<BookingResponse>> MarkNoShow(
        Guid organizationId,
        Guid bookingId,
        CancellationToken cancellationToken)
    {
        return ChangeStatus(
            organizationId,
            bookingId,
            BookingTransition.MarkNoShow,
            cancellationToken);
    }

    [HttpPost("{bookingId:guid}/reschedule")]
    public async Task<ActionResult<BookingResponse>> Reschedule(
        Guid organizationId,
        Guid bookingId,
        RescheduleBookingRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await _rescheduleBookingHandler.HandleAsync(
                new RescheduleBookingCommand(
                    organizationId,
                    bookingId,
                    request.StartsAtUtc),
                cancellationToken);

        return Ok(Map(result));
    }

    private async Task<ActionResult<BookingResponse>> ChangeStatus(
        Guid organizationId,
        Guid bookingId,
        BookingTransition transition,
        CancellationToken cancellationToken)
    {
        var result =
            await _changeBookingStatusHandler.HandleAsync(
                new ChangeBookingStatusCommand(
                    organizationId,
                    bookingId,
                    transition),
                cancellationToken);

        return Ok(Map(result));
    }

    private static BookingResponse Map(
        BookingDetails booking)
    {
        return new BookingResponse(
            booking.Id,
            booking.OrganizationId,
            booking.CustomerId,
            booking.EmployeeId,
            booking.ServiceId,
            booking.StartsAtUtc,
            booking.EndsAtUtc,
            booking.PriceAmount,
            booking.Currency,
            booking.Notes,
            booking.Status,
            booking.CreatedAtUtc,
            booking.UpdatedAtUtc,
            booking.CancelledAtUtc,
            booking.CompletedAtUtc,
            booking.NoShowAtUtc);
    }
}
