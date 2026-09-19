using BookingHub.Application.Abstractions;

namespace BookingHub.Infrastructure.Identifiers;

internal sealed class GuidGenerator : IGuidGenerator
{
    public Guid NewGuid()
    {
        return Guid.NewGuid();
    }
}
