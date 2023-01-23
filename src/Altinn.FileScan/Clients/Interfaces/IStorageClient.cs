using Altinn.Platform.Storage.Interface.Enums;
using Altinn.Platform.Storage.Interface.Models;

namespace Altinn.FileScan.Clients.Interfaces
{
    /// <summary>
    /// Interface containing all client actions for the Altinn Storage Client
    /// </summary>
    public interface IStorageClient
    {
        /// <summary>
        /// Sends a request to Altinn Storage requesting to update the file scan result of a data element
        /// </summary>
        /// <returns></returns>
        Task<bool> PatchDataElementFileScanResult(string dataElementId, FileScanStatus fileScanStatus);
    }
}
