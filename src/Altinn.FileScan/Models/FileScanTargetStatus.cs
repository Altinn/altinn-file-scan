namespace Altinn.FileScan.Models;

/// <summary>
/// Whether Altinn Storage says a blob version awaits its file scan, and if not, why. Storage sends it as a
/// camel-cased string, which <see cref="Clients.StorageClient"/> requires.
/// </summary>
public enum FileScanTargetStatus
{
    /// <summary>
    /// The version is the data element's current version and its scan is pending.
    /// </summary>
    Pending,

    /// <summary>
    /// A newer blob version has replaced this one; the newer version's scan reports for the data element.
    /// </summary>
    Superseded,

    /// <summary>
    /// The version is current, but its file scan result is no longer pending.
    /// </summary>
    NotPending,

    /// <summary>
    /// The data element no longer exists.
    /// </summary>
    Deleted,
}
