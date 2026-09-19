namespace BookingHub.Api.Realtime;

public static class RealtimeGroupNames
{
    public static string Organization(Guid organizationId)
    {
        return $"organization:{organizationId}";
    }

    public static string User(Guid userId)
    {
        return $"user:{userId}";
    }
}
