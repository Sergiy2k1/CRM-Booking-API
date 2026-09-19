namespace BookingHub.Api.Contracts.Authentication;

public sealed record RefreshSessionRequest(
    string RefreshToken);
