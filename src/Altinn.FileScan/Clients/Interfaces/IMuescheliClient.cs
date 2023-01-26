using Altinn.FileScan.Models;

namespace Altinn.FileScan.Clients.Interfaces
{
    /// <summary>
    /// Interface containing all client actions for the Altinn Muescheli Client
    /// </summary>
    public interface IMuescheliClient
    {
        /// <summary>
        /// Send a request to scan the file provided in the stream
        /// </summary>
        /// <returns>The malware scan result</returns>
        public Task<ScanResult> ScanStream(Stream stream, string filename);
    }
}
