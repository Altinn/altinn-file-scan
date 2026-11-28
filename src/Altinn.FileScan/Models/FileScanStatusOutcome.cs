namespace Altinn.FileScan.Models;

/// <summary>
/// Whether Altinn Storage set a file scan status, and if not, why. Storage sends it as a camel-cased string, which
/// <see cref="Clients.StorageClient"/> requires.
/// </summary>
public enum FileScanStatusOutcome
{
    /// <summary>
    /// The file scan status was set.
    /// </summary>
    Updated,

    /// <summary>
    /// The scanned blob version is no longer the data element's current version, so the status was not set.
    /// </summary>
    Superseded,

    /// <summary>
    /// The data element or its instance no longer exists.
    /// </summary>
    Deleted,
}
