namespace BookingHub.Application.Abstractions.Authentication;

public interface IRefreshTokenProvider
{
    GeneratedRefreshToken Generate(
        DateTimeOffset issuedAtUtc);

    string Hash(string token);
}
