using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Application.Services.Common;

namespace BookingHub.Application.Services.ListServices;

public sealed class ListServicesHandler
{
    public const int MaxPageSize = 100;

    private readonly IServiceRepository _serviceRepository;

    public ListServicesHandler(
        IServiceRepository serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    public async Task<ListServicesResult> HandleAsync(
        ListServicesQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.Page < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(query),
                "Page must be at least 1.");
        }

        if (query.PageSize < 1 ||
            query.PageSize > MaxPageSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(query),
                $"Page size must be between 1 and {MaxPageSize}.");
        }

        if (query.Status.HasValue &&
            !Enum.IsDefined(query.Status.Value))
        {
            throw new ArgumentOutOfRangeException(
                nameof(query),
                "Service status is invalid.");
        }

        var skip =
            (query.Page - 1) * query.PageSize;

        var services =
            await _serviceRepository.ListAsync(
                query.OrganizationId,
                query.Status,
                skip,
                query.PageSize,
                cancellationToken);

        var totalCount =
            await _serviceRepository.CountAsync(
                query.OrganizationId,
                query.Status,
                cancellationToken);

        return new ListServicesResult(
            services.Select(x => x.ToDetails()).ToArray(),
            query.Page,
            query.PageSize,
            totalCount);
    }
}
