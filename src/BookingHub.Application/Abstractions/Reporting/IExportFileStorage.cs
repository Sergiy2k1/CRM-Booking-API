namespace BookingHub.Application.Abstractions.Reporting;

public interface IExportFileStorage
{
    Task SaveTextAsync(
        string storageKey,
        string content,
        CancellationToken cancellationToken = default);

    Task<Stream> OpenReadAsync(
        string storageKey,
        CancellationToken cancellationToken = default);
}
