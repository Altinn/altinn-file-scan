using System.Threading.Tasks;

namespace Altinn.FileScan.Functions.Clients.Interfaces
{
    /// <summary>
    /// Interface to FileScan API
    /// </summary>
    public interface IFileScanClient
    {
        /// <summary>
        /// Send dataElement for file scanning.
        /// </summary>
        /// <param name="dataElementScanRequest">DataElement to send</param>
        Task PostDataElementScanRequest(string dataElementScanRequest);
    }
}
