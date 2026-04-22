using Altinn.FileScan.Models;

namespace Altinn.FileScan.Exceptions;

/// <summary>
/// An exception class related to non expected http response from the Muescheli Service
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MuescheliHttpException"/> class.
/// </remarks>
public class MuescheliScanResultException(string dataElementId, ScanResult result, string message) : Exception(message)
{
    /// <summary>
    /// The id of the data element related to the exception
    /// </summary>
    public string DataElementId { get; } = dataElementId;

    /// <summary>
    /// The scan result that generated the exception
    /// </summary>
    public ScanResult ScanResult { get; } = result;

    /// <summary>
    /// Creates a new <see cref="MuescheliScanResultException"/> combining the response message and
    /// </summary>
    public static MuescheliScanResultException Create(string dataElementId, ScanResult result)
    {
        string message = $"Muescheli scan returned result code `{result}` for data element with id {dataElementId}";
        return new MuescheliScanResultException(dataElementId, result, message);
    }
}
