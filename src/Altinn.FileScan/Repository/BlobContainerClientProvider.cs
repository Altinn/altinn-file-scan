using System.Collections.Concurrent;
using System.Text.Json;

using Altinn.FileScan.Configuration;
using Altinn.FileScan.Repository.Interfaces;
using Altinn.FileScan.Services.Interfaces;

using Azure.Storage;
using Azure.Storage.Blobs;

using Microsoft.Extensions.Options;

namespace Altinn.FileScan.Repository
{
    /// <summary>
    /// Represents a collection of Azure Blob Container Clients and the means to obtain new tokens when needed.
    /// This class should be used as a singleton through dependency injection.
    /// </summary>
    public class BlobContainerClientProvider : IBlobContainerClientProvider
    {
        private readonly ConcurrentDictionary<string, (DateTime Created, BlobContainerClient Client)> _containerClients =
            new();

        private readonly AppOwnerAzureStorageConfig _storageConfig;
        private readonly IAppOwnerKeyVault _keyVault;
        private readonly ILogger<IBlobContainerClientProvider> _logger;

        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly Dictionary<string, string> _orgKeyVaultDict;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobContainerClientProvider"/> class/>.
        /// </summary>       
        public BlobContainerClientProvider(
            IAppOwnerKeyVault keyVault,
            IOptions<AppOwnerAzureStorageConfig> storageConfiguration,
            ILogger<IBlobContainerClientProvider> logger)
        {
            _keyVault = keyVault;
            _storageConfig = storageConfiguration.Value;
            _logger = logger;
            _orgKeyVaultDict = JsonSerializer.Deserialize<Dictionary<string, string>>(_storageConfig.OrgKeyVaultDict);
        }

        /// <inheritdoc/>
        public async Task<BlobContainerClient> GetBlobContainerClient(string org, int? storageContainerNumber)
        {
            string clientsKey = $"{org}-{storageContainerNumber}";
            if (_containerClients.TryGetValue(clientsKey, out (DateTime Created, BlobContainerClient Client) containerClient) && StillYoung(containerClient.Created))
            {
                return containerClient.Client;
            }

            _containerClients.TryRemove(clientsKey, out _);

            await _semaphore.WaitAsync();
            try
            {
                BlobContainerClient blobContainerClient;

                if (_storageConfig.AccountName == "devstoreaccount1")
                {
                    StorageSharedKeyCredential storageCredentials = new(_storageConfig.AccountName, _storageConfig.AccountKey);
                    Uri storageUrl = new(_storageConfig.BlobEndPoint);
                    BlobServiceClient commonBlobClient = new(storageUrl, storageCredentials);
                    blobContainerClient = commonBlobClient.GetBlobContainerClient(_storageConfig.StorageContainer);
                }
                else
                {
                    var containerUri = await GetBlobUri(org, storageContainerNumber);
                    blobContainerClient = new BlobContainerClient(containerUri);
                }

                _containerClients.TryAdd(clientsKey, (DateTime.UtcNow, blobContainerClient));
                return blobContainerClient;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Generates a container uri for an app owner blob container
        /// </summary>
        internal async Task<Uri> GetBlobUri(string org, int? storageContainerNumber)
        {
            string sasToken = await GetSasToken(org);
            string accountName = string.Format(_storageConfig.OrgStorageAccount, org);
            string containerName = string.Format(_storageConfig.OrgStorageContainer, org)
                + (storageContainerNumber != null ? $"-{storageContainerNumber}" : null);

            UriBuilder fullUri = new()
            {
                Scheme = "https",
                Host = $"{accountName}.blob.core.windows.net",
                Path = $"{containerName}",
                Query = sasToken
            };

            return fullUri.Uri;
        }

        private bool StillYoung(DateTime created)
        {
            return created.AddHours(_storageConfig.AllowedSasTokenAgeHours) > DateTime.UtcNow;
        }

        private async Task<string> GetSasToken(string org)
        {
            string storageAccount = string.Format(_storageConfig.OrgStorageAccount, org);
            string sasDefinition = string.Format(_storageConfig.OrgSasDefinition, org);

            string secretName = $"{storageAccount}-{sasDefinition}";

            if (_orgKeyVaultDict.TryGetValue(org, out string keyVaultUri))
            {
                // key was found in dictionary and keyVaultUri populated with a value
            }
            else
            {
                keyVaultUri = string.Format(_storageConfig.OrgKeyVaultURI, org);
            }

            return await _keyVault.GetSecretAsync(keyVaultUri, secretName);
        }
    }
}
