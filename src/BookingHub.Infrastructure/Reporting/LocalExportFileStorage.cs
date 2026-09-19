using System.Text;
using BookingHub.Application.Abstractions.Reporting;
using Microsoft.Extensions.Configuration;

namespace BookingHub.Infrastructure.Reporting;

public sealed class LocalExportFileStorage
    : IExportFileStorage
{
    private static readonly Encoding Utf8WithoutBom =
        new UTF8Encoding(
            encoderShouldEmitUTF8Identifier: false);

    private readonly string _rootPath;

    public LocalExportFileStorage(
        IConfiguration configuration)
    {
        var configuredPath =
            configuration["ExportStorage:RootPath"];

        _rootPath =
            Path.GetFullPath(
                string.IsNullOrWhiteSpace(configuredPath)
                    ? Path.Combine(
                        AppContext.BaseDirectory,
                        "data",
                        "exports")
                    : configuredPath);

        Directory.CreateDirectory(
            _rootPath);
    }

    public async Task SaveTextAsync(
        string storageKey,
        string content,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            storageKey);

        ArgumentNullException.ThrowIfNull(
            content);

        var fullPath =
            ResolvePath(
                storageKey);

        var directory =
            Path.GetDirectoryName(
                fullPath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(
                directory);
        }

        await File.WriteAllTextAsync(
            fullPath,
            content,
            Utf8WithoutBom,
            cancellationToken);
    }

    public Task<Stream> OpenReadAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var fullPath =
            ResolvePath(
                storageKey);

        Stream stream =
            new FileStream(
                fullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 4096,
                options:
                    FileOptions.Asynchronous |
                    FileOptions.SequentialScan);

        return Task.FromResult(
            stream);
    }

    private string ResolvePath(
        string storageKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            storageKey);

        var fullPath =
            Path.GetFullPath(
                Path.Combine(
                    _rootPath,
                    storageKey));

        var rootWithSeparator =
            _rootPath.EndsWith(
                Path.DirectorySeparatorChar)
                ? _rootPath
                : _rootPath + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(
                rootWithSeparator,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Export storage key resolved outside the configured root.");
        }

        return fullPath;
    }
}
