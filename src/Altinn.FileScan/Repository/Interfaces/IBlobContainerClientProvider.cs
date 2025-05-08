using Azure.Storage.Blobs;

namespace Altinn.FileScan.Repository.Interfaces
{
    /// <summary>
    /// This interface describes a component able to obtain and invalidate a blob client for operations on an Azure storage account.
    /// </summary>
    public interface IBlobContainerClientProvider
    {
        /// <summary>
        /// Get the container client to access blobs in the storage account for given application owner.
        /// </summary>
        /// <param name="org">The application owner id.</param>
        /// <param name="storageAccountNumber">Alternate number to append to container name</param>
        /// <returns>The container client to use when accessing the application owner storage account.</returns>
        BlobContainerClient GetBlobContainerClient(string org, int? storageAccountNumber);
    }
}
