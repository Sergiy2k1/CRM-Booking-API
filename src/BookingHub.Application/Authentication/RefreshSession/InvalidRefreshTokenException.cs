namespace BookingHub.Application.Authentication.RefreshSession;

public sealed class InvalidRefreshTokenException : Exception
{
    public InvalidRefreshTokenException()
        : base("Refresh token is invalid or expired.")
    {
    }
}
