using Altinn.FileScan.Configuration;
using Altinn.FileScan.Repository.Interfaces;

using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

using Microsoft.Extensions.Options;

namespace Altinn.FileScan.Repository
{
    /// <summary>
    /// Implementation of IAppOwnerBlob towards Azure Storage
    /// </summary>
    public class AppOwnerBlobRepository : IAppOwnerBlob
    {
        private readonly AppOwnerAzureStorageConfig _storageConfig;
        private readonly ISasTokenProvider _sasTokenProvider;
        private readonly ILogger<IAppOwnerBlob> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppOwnerBlobRepository"/> class.
        /// </summary>
        public AppOwnerBlobRepository(ISasTokenProvider sasTokenProvider, IOptions<AppOwnerAzureStorageConfig> storageConfig, ILogger<IAppOwnerBlob> logger)
        {
            _sasTokenProvider = sasTokenProvider;
            _storageConfig = storageConfig.Value;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<Stream> GetBlob(string org, string blobPath)
        {
            BlobClient blockBlob = await CreateBlobClient(org, blobPath);

            _logger.LogInformation("// AppOwnerBlobRepository // GetBlob // Blob client created ok");
            Azure.Response<BlobDownloadInfo> response = await blockBlob.DownloadAsync();

            return response.Value.Content;
        }

        private async Task<BlobClient> CreateBlobClient(string org, string blobPath)
        {
            if (!_storageConfig.AccountName.StartsWith("devstoreaccount1"))
            {
                string sasToken = await _sasTokenProvider.GetSasToken(org);

                string accountName = string.Format(_storageConfig.OrgStorageAccount, org);
                string containerName = string.Format(_storageConfig.OrgStorageContainer, org);

                UriBuilder fullUri = new UriBuilder
                {
                    Scheme = "https",
                    Host = $"{accountName}.blob.core.windows.net",
                    Path = $"{containerName}/{blobPath}",
                    Query = sasToken
                };

                return new BlobClient(fullUri.Uri);
            }

            StorageSharedKeyCredential storageCredentials = new StorageSharedKeyCredential(_storageConfig.AccountName, _storageConfig.AccountKey);
            Uri storageUrl = new Uri(_storageConfig.BlobEndPoint);
            BlobServiceClient commonBlobClient = new BlobServiceClient(storageUrl, storageCredentials);
            BlobContainerClient blobContainerClient = commonBlobClient.GetBlobContainerClient(_storageConfig.StorageContainer);

            return blobContainerClient.GetBlobClient(blobPath);
        }
    }
}
