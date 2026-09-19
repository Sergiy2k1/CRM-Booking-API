using BookingHub.Application.Authentication.Login;
using BookingHub.Application.Bookings.CreateBooking;
using BookingHub.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BookingHub.Api.ErrorHandling;

public sealed class ApiExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails =
            CreateProblemDetails(exception);

        httpContext.Response.StatusCode =
            problemDetails.Status
            ?? StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken);

        return true;
    }

    private static ProblemDetails CreateProblemDetails(
        Exception exception)
    {
        return exception switch
        {
            InvalidCredentialsException =>
                new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Invalid credentials",
                    Detail = "The supplied credentials are invalid."
                },

            EntityNotFoundException notFoundException =>
                new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Resource not found",
                    Detail = notFoundException.Message
                },

            BookingUnavailableException unavailableException =>
                CreateBookingUnavailableProblem(
                    unavailableException),

            ArgumentException argumentException =>
                new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Invalid request",
                    Detail = argumentException.Message
                },

            InvalidOperationException invalidOperationException =>
                new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Operation conflict",
                    Detail = invalidOperationException.Message
                },

            _ =>
                new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Internal server error",
                    Detail = "An unexpected error occurred."
                }
        };
    }

    private static ProblemDetails CreateBookingUnavailableProblem(
        BookingUnavailableException exception)
    {
        var problemDetails =
            new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Booking slot unavailable",
                Detail = exception.Message
            };

        problemDetails.Extensions["availabilityStatus"] =
            exception.AvailabilityStatus.ToString();

        return problemDetails;
    }
}
