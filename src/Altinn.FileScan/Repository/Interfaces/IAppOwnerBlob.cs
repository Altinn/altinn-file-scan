using Altinn.FileScan.Models;

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
        /// <param name="storageContainerNumber">Alternate number to append to container name</param>
        /// <returns>The blob as a stream</returns>W
        public Task<Stream> GetBlob(string org, string blobPath, int? storageContainerNumber);

        /// <summary>
        /// Retrieves the blob metadata as stored by Blob Storage
        /// </summary>
        /// <param name="org">The short name of the organisation</param>
        /// <param name="blobPath">Full path to the blob within a storage account</param>
        /// <param name="storageContainerNumber">Alternate number to append to container name</param>
        /// <returns>Blob properties object</returns>
        public Task<BlobPropertyModel> GetBlobProperties(string org, string blobPath, int? storageContainerNumber);
    }
}
