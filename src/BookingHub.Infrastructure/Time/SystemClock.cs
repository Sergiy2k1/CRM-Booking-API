using BookingHub.Application.Abstractions;

namespace BookingHub.Infrastructure.Time;

internal sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow =>
        DateTimeOffset.UtcNow;
}
