namespace Altinn.FileScan.Models
{
    /// <summary>
    /// Definition of the response object from the Muescheli service
    /// </summary>
    public class MuescheliResponse
    {
        /// <summary>
        /// Gets or sets the name of the file in the response object
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Gets or sets the scan result in the the response object
        /// </summary>       
        public ScanResult Result { get; set; }
    }
}
