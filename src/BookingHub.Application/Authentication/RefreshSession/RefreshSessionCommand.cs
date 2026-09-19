namespace BookingHub.Application.Authentication.RefreshSession;

public sealed record RefreshSessionCommand(
    string RefreshToken);
