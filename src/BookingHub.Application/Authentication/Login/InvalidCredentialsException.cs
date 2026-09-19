namespace BookingHub.Application.Authentication.Login;

public sealed class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException()
        : base("Invalid email, password, or organization access.")
    {
    }
}
