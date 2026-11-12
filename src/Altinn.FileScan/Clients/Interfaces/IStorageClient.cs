using System.Threading.Tasks;
using Altinn.FileScan.Models;
using Altinn.Platform.Storage.Interface.Models;

namespace Altinn.FileScan.Clients.Interfaces;

/// <summary>
/// Interface containing all client actions for the Altinn Storage Client
/// </summary>
public interface IStorageClient
{
    /// <summary>
    /// Sends a request to Altinn Storage requesting to update the file scan status of a data element
    /// </summary>
    /// <returns>Whether Storage set the status, and if not, why.</returns>
    /// <exception cref="Exceptions.PlatformHttpException">Storage did not answer as expected.</exception>
    Task<FileScanStatusOutcome> PatchFileScanStatus(
        string instanceId,
        string dataElementId,
        FileScanStatus fileScanStatus
    );

    /// <summary>
    /// Gets the blob of a scan request from Altinn Storage, unless the blob version no longer awaits its scan.
    /// </summary>
    /// <param name="instanceId">Instance id</param>
    /// <param name="dataElementId">Data element id</param>
    /// <param name="blobVersionId">The blob version the scan request is for</param>
    /// <returns>The blob, which the caller disposes, or why there is nothing to scan.</returns>
    /// <exception cref="Exceptions.PlatformHttpException">
    /// Storage did not answer as expected, or the blob could not be downloaded. Retrying may succeed.
    /// </exception>
    Task<BlobToScan> GetBlobToScan(string instanceId, string dataElementId, string blobVersionId);
}
