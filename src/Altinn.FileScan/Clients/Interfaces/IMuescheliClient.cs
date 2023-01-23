using Altinn.FileScan.Models;

namespace Altinn.FileScan.Clients.Interfaces
{
    /// <summary>
    /// Describes the implementation of a Clam AV client
    /// </summary>
    public interface IMuescheliClient
    {
        /// <summary>
        /// Sends a file stream to the clam AV component for scanning
        /// </summary>
        /// <returns>The malware scan result</returns>
        public Task<ScanResult> ScanStream(Stream stream, string filename);
    }
}
