#nullable disable

namespace Altinn.FileScan.Models;

/// <summary>
/// This class represents a request to perform a file scan of an Altinn Data Element.
/// </summary>
public class DataElementScanRequest
{
    /// <summary>
    /// Gets or sets the unique id of the data element.
    /// </summary>
    public string DataElementId { get; set; }

    /// <summary>
    /// Gets or sets the unique id of the parent instance of the data element.
    /// </summary>
    /// <remarks>
    /// The instance id contains both the instance owner party id and the unique instance guid.
    /// </remarks>
    public string InstanceId { get; set; }

    /// <summary>
    /// Gets or sets the name of the data element (file)
    /// </summary>
    public string Filename { get; set; }

    /// <summary>
    /// Gets or sets the id of the blob version to scan, which is read through a read URL from Altinn Storage.
    /// </summary>
    public string BlobVersionId { get; set; }
}
