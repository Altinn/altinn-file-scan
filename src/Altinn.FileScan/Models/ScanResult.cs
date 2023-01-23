namespace Altinn.FileScan.Models
{
    /// <summary>
    /// Enum for malware scan results
    /// </summary>
    public enum ScanResult
    {
        UNDEFINED,
        OK,
        FOUND,
        ERROR,
        PARSE_ERROR
    }
}
