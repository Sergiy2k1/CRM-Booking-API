using BookingHub.Api.Contracts.Authentication;
using BookingHub.Application.Authentication.Login;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingHub.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly LoginHandler _loginHandler;

    public AuthController(LoginHandler loginHandler)
    {
        _loginHandler = loginHandler;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<LoginResponse>(
        StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await _loginHandler.HandleAsync(
                new LoginCommand(
                    request.Email,
                    request.Password,
                    request.OrganizationId),
                cancellationToken);

        return Ok(
            new LoginResponse(
                result.AccessToken,
                result.ExpiresAtUtc,
                result.UserId,
                result.OrganizationId,
                result.Role));
    }
}
