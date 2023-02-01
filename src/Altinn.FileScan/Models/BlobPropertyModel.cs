namespace Altinn.FileScan.Models
{
    /// <summary>
    /// Model type containing selected properties from Azure BlobProperties type
    /// </summary>
    public class BlobPropertyModel
    {
        /// <summary>
        /// Gets or sets when the blob was last modified
        /// </summary>
        public DateTimeOffset LastModified { get; set; }
    }
}
