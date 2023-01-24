using Altinn.FileScan.Models;

namespace Altinn.FileScan.Exceptions
{
    /// <summary>
    /// An exception class related to non expected http response from the Muescheli Service
    /// </summary>
    [Serializable]
    public class MuescheliScanResultException : Exception
    {
        /// <summary>
        /// The id of the data element related to the exception
        /// </summary>
        public string DataElementId { get; }

        /// <summary>
        /// The scan result that generated the exception
        /// </summary>
        public ScanResult ScanResult { get; }

        /// <summary>
        /// Creates a new <see cref="MuescheliScanResultException"/> combining the response message and 
        /// </summary>
        public static MuescheliScanResultException Create(string dataElementId, ScanResult result)
        {
            string message = $"Muescheli scan returned result code `{result}` for data element with id {dataElementId}";
            return new MuescheliScanResultException(dataElementId, result, message);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MuescheliHttpException"/> class.
        /// </summary>
        public MuescheliScanResultException(string dataElementId, ScanResult result, string message) : base(message)
        {
            DataElementId = dataElementId;
            ScanResult = result;
        }
    }
}
