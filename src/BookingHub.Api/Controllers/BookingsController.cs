using BookingHub.Api.Contracts.Bookings;
using BookingHub.Application.Bookings.CreateBooking;
using Microsoft.AspNetCore.Mvc;

namespace BookingHub.Api.Controllers;

[ApiController]
[Route("api/organizations/{organizationId:guid}/bookings")]
public sealed class BookingsController : ControllerBase
{
    private readonly CreateBookingHandler _createBookingHandler;

    public BookingsController(
        CreateBookingHandler createBookingHandler)
    {
        _createBookingHandler = createBookingHandler;
    }

    [HttpPost]
    [ProducesResponseType<CreateBookingResponse>(
        StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreateBookingResponse>> Create(
        Guid organizationId,
        CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        var command =
            new CreateBookingCommand(
                organizationId,
                request.CustomerId,
                request.EmployeeId,
                request.ServiceId,
                request.StartsAtUtc,
                request.Notes);

        var result =
            await _createBookingHandler.HandleAsync(
                command,
                cancellationToken);

        var response =
            new CreateBookingResponse(
                result.BookingId,
                result.Status,
                result.StartsAtUtc,
                result.EndsAtUtc,
                result.PriceAmount,
                result.Currency);

        return Created(
            $"/api/organizations/{organizationId}/bookings/{result.BookingId}",
            response);
    }
}
