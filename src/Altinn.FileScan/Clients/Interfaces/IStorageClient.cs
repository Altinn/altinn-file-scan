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
    Task PatchFileScanStatus(string instanceId, string dataElementId, FileScanStatus fileScanStatus);

    /// <summary>
    /// Checks if the data element exists in the storage database
    /// </summary>
    /// <param name="instanceId">Instance id</param>
    /// <param name="dataElementId">Data element id</param>
    /// <returns>True if the data element exists, false otherwise</returns>
    Task<bool> DataElementExists(string instanceId, string dataElementId);
}
