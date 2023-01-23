namespace Altinn.FileScan.Models
{
    /// <summary>
    /// Definition of the response object from the muecheli service
    /// </summary>
    public class MuescheliResponse
    {
        /// <summary>
        /// Gets or sets the name of the file that has been scanned
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Gets or sets the result of the malware scan as a scan result enum
        /// </summary>       
        public ScanResult Result { get; set; }
    }
}
