using System.Runtime.Serialization;

namespace Altinn.FileScan.Configuration
{
    /// <summary>
    /// Represents a set of configuration options when communicating with the Altinn Platform API.
    /// </summary>
    public class PlatformSettings
    {
        /// <summary>
        /// Gets or sets the url for the Storage API endpoint.
        /// </summary>
        public string ApiStorageEndpoint { get; set; }

        /// <summary>
        /// Sets or sets the url for the Muescheli API endpoint
        /// </summary>
        public string ApiMuescheliEndpoint { get;  set; }
    }
}
