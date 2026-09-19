namespace BookingHub.Application.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
