using Altinn.FileScan.Models;
using Altinn.FileScan.Repository.Interfaces;

using Azure.Storage.Blobs.Models;

namespace Altinn.FileScan.Repository
{
    /// <summary>
    /// Implementation of IAppOwnerBlob towards Azure Storage
    /// </summary>
    public class AppOwnerBlobRepository : IAppOwnerBlob
    {
        private readonly IBlobContainerClientProvider _containerClientProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppOwnerBlobRepository"/> class.
        /// </summary>
        public AppOwnerBlobRepository(IBlobContainerClientProvider containerClientProvider)
        {
            _containerClientProvider = containerClientProvider;
        }

        /// <inheritdoc/>
        public async Task<Stream> GetBlob(string org, string blobPath, int? storageContainerNumber)
        {
            var containerClient = await _containerClientProvider.GetBlobContainerClient(org, storageContainerNumber);
            var blobClient = containerClient.GetBlobClient(blobPath);
            Azure.Response<BlobDownloadInfo> response = await blobClient.DownloadAsync();
            return response.Value.Content;
        }

        /// <inheritdoc/>
        public async Task<BlobPropertyModel> GetBlobProperties(string org, string blobPath, int? storageContainerNumber)
        {
            var containerClient = await _containerClientProvider.GetBlobContainerClient(org, storageContainerNumber);
            var blobClient = containerClient.GetBlobClient(blobPath);
            Azure.Response<BlobProperties> response = await blobClient.GetPropertiesAsync();
            return new BlobPropertyModel { LastModified = response.Value.LastModified };
        }
    }
}
