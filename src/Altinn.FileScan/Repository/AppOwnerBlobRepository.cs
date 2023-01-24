﻿using Altinn.FileScan.Configuration;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="AppOwnerBlobRepository"/> class.
        /// </summary>
        public AppOwnerBlobRepository(ISasTokenProvider sasTokenProvider, IOptions<AppOwnerAzureStorageConfig> storageConfig)
        {
            _sasTokenProvider = sasTokenProvider;
            _storageConfig = storageConfig.Value;
        }

        /// <inheritdoc/>
        public async Task<(bool Success, Stream BlobStream)> GetBlob(string org, string blobPath)
        {
            BlobClient blockBlob = await CreateBlobClient(org, blobPath);

            Azure.Response<BlobDownloadInfo> response = await blockBlob.DownloadAsync();
            return (true, response.Value.Content);
        }

        private async Task<BlobClient> CreateBlobClient(string org, string blobPath)
        {
            if (_storageConfig.AccountName == "devstoreaccount1")
            {
                StorageSharedKeyCredential storageCredentials = new(_storageConfig.AccountName, _storageConfig.AccountKey);
                Uri storageUrl = new(_storageConfig.BlobEndPoint);
                BlobServiceClient commonBlobClient = new(storageUrl, storageCredentials);
                BlobContainerClient blobContainerClient = commonBlobClient.GetBlobContainerClient(_storageConfig.StorageContainer);

                return blobContainerClient.GetBlobClient(blobPath);
            }

            string sasToken = await _sasTokenProvider.GetSasToken(org);
            string accountName = string.Format(_storageConfig.OrgStorageAccount, org);
            string containerName = string.Format(_storageConfig.OrgStorageContainer, org);

            UriBuilder fullUri = new()
            {
                Scheme = "https",
                Host = $"{accountName}.blob.core.windows.net",
                Path = $"{containerName}/{blobPath}",
                Query = sasToken
            };

            return new BlobClient(fullUri.Uri);
        }
    }
}
