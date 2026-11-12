using System;
using System.IO;

namespace Altinn.FileScan.Models;

/// <summary>
/// The blob of a scan request, or why Altinn Storage says there is nothing to scan. Disposing it closes the
/// download.
/// </summary>
public sealed class BlobToScan : IDisposable
{
    private readonly IDisposable? _download;

    private BlobToScan(FileScanTargetStatus status, Stream? content, IDisposable? download)
    {
        Status = status;
        Content = content;
        _download = download;
    }

    /// <summary>
    /// Gets whether the blob version awaits its scan, and if not, why.
    /// </summary>
    public FileScanTargetStatus Status { get; }

    /// <summary>
    /// Gets the blob content when <see cref="Status"/> is <see cref="FileScanTargetStatus.Pending"/>.
    /// </summary>
    public Stream? Content { get; }

    /// <summary>
    /// Creates the result for a blob version that no longer awaits its scan.
    /// </summary>
    /// <param name="status">Why the blob version doesn't await its scan.</param>
    public static BlobToScan NotAwaitingScan(FileScanTargetStatus status)
    {
        if (status == FileScanTargetStatus.Pending)
        {
            throw new ArgumentOutOfRangeException(nameof(status), status, "A pending blob has content.");
        }

        return new BlobToScan(status, null, null);
    }

    /// <summary>
    /// Creates the result for a blob version that awaits its scan.
    /// </summary>
    /// <param name="content">The blob content.</param>
    /// <param name="download">What to dispose with the content, such as the response it is read from.</param>
    public static BlobToScan Pending(Stream content, IDisposable? download = null)
    {
        return new BlobToScan(FileScanTargetStatus.Pending, content, download);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Content?.Dispose();
        _download?.Dispose();
    }
}
