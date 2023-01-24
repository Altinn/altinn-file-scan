namespace Altinn.FileScan.Repository.Interfaces
{
    /// <summary>
    /// Interface containing all repository actions for an Altinn app owner Azure blob repository
    /// </summary>
    public interface IAppOwnerBlob
    {
        /// <summary>
        /// Retrieves a data blob as a stream from an app owner storage account within Altinn
        /// </summary>
        /// <param name="org">The short name of the organisation</param>
        /// <param name="blobPath">Full path to the blob within a storage account</param>
        /// <param name="contentHash">The content hash of the data element</param>
        /// <returns>The blob as a stream</returns>W
        public Task<(bool Success, Stream BlobStream)> GetBlob(string org, string blobPath, string contentHash);
    }
}
