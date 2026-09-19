using BookingHub.Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Identity;

namespace BookingHub.Infrastructure.Authentication;

internal sealed class PasswordHasher : IPasswordHasher
{
    private static readonly object PasswordHasherUser = new();

    private readonly PasswordHasher<object> _passwordHasher = new();

    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        return _passwordHasher.HashPassword(
            PasswordHasherUser,
            password);
    }

    public bool Verify(
        string passwordHash,
        string providedPassword)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(providedPassword);

        var result =
            _passwordHasher.VerifyHashedPassword(
                PasswordHasherUser,
                passwordHash,
                providedPassword);

        return result is
            PasswordVerificationResult.Success or
            PasswordVerificationResult.SuccessRehashNeeded;
    }
}
