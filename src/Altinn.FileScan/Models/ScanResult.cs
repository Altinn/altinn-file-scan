namespace Altinn.FileScan.Models;

/// <summary>
/// Enum for possible responses from the malware scan as being presented by the Muescheli container.
/// </summary>
public enum ScanResult
{
    /// <summary>
    /// The result of the scan is unknown.
    /// </summary>
    UNDEFINED,

    /// <summary>
    /// The scan didn't find any malware.
    /// </summary>
    OK,

    /// <summary>
    /// The scan identified malware.
    /// </summary>
    FOUND,

    /// <summary>
    /// An error occured.
    /// </summary>
    ERROR,

    /// <summary>
    /// An error occured.
    /// </summary>
    PARSE_ERROR
}
