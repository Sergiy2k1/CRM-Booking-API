using BookingHub.Api.Contracts.Authentication;
using BookingHub.Application.Authentication.Login;
using BookingHub.Application.Authentication.RefreshSession;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BookingHub.Api.Controllers;

[ApiController]
[EnableRateLimiting("auth")]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly LoginHandler _loginHandler;
    private readonly RefreshSessionHandler _refreshSessionHandler;

    public AuthController(
        LoginHandler loginHandler,
        RefreshSessionHandler refreshSessionHandler)
    {
        _loginHandler = loginHandler;
        _refreshSessionHandler = refreshSessionHandler;
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
                result.AccessTokenExpiresAtUtc,
                result.RefreshToken,
                result.RefreshTokenExpiresAtUtc,
                result.UserId,
                result.OrganizationId,
                result.Role));
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType<RefreshSessionResponse>(
        StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<RefreshSessionResponse>> Refresh(
        RefreshSessionRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await _refreshSessionHandler.HandleAsync(
                new RefreshSessionCommand(
                    request.RefreshToken),
                cancellationToken);

        return Ok(
            new RefreshSessionResponse(
                result.AccessToken,
                result.AccessTokenExpiresAtUtc,
                result.RefreshToken,
                result.RefreshTokenExpiresAtUtc,
                result.UserId,
                result.OrganizationId,
                result.Role));
    }
}
