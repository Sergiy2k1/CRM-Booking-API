namespace BookingHub.Api.Contracts.Bookings;

public sealed record BookingListResponse(
    IReadOnlyCollection<BookingResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
