using Altinn.FileScan.Configuration;
using Altinn.FileScan.Repository.Interfaces;
using Azure.Core;
using Azure.Identity;
using Azure.Storage;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Altinn.FileScan.Repository
{
    /// <summary>
    /// Represents a collection of Azure Blob Container Clients and the means to obtain new tokens when needed.
    /// This class should be used as a singleton through dependency injection.
    /// </summary>
    public class BlobContainerClientProvider : IBlobContainerClientProvider
    {
        private const string _credsCacheKey = "creds";

        private readonly AppOwnerAzureStorageConfig _storageConfig;
        private readonly ILogger<BlobContainerClientProvider> _logger;

        private readonly IMemoryCache _memoryCache;

        private static readonly MemoryCacheEntryOptions _cacheEntryOptionsCreds = new MemoryCacheEntryOptions()
            .SetPriority(CacheItemPriority.High)
            .SetAbsoluteExpiration(new TimeSpan(10, 0, 0));

        private static readonly MemoryCacheEntryOptions _cacheEntryOptionsBlobClient = new MemoryCacheEntryOptions()
            .SetPriority(CacheItemPriority.High)
            .SetAbsoluteExpiration(new TimeSpan(10, 0, 0));

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobContainerClientProvider"/> class/>.
        /// </summary>       
        public BlobContainerClientProvider(
            IOptions<AppOwnerAzureStorageConfig> storageConfiguration,
            ILogger<BlobContainerClientProvider> logger,
            IMemoryCache memoryCache)
        {
            _storageConfig = storageConfiguration.Value;
            _logger = logger;
            _memoryCache = memoryCache;
        }

        /// <inheritdoc/>
        public BlobContainerClient GetBlobContainerClient(string org, int? storageAccountNumber)
        {
            if (!_storageConfig.OrgStorageAccount.Equals("devstoreaccount1"))
            {
                string cacheKey = GetClientCacheKey(org, storageAccountNumber);
                if (!_memoryCache.TryGetValue(cacheKey, out BlobContainerClient client))
                {
                    string containerName = string.Format(_storageConfig.StorageContainer, org);
                    string accountName = string.Format(_storageConfig.OrgStorageAccount, org);
                    if (storageAccountNumber != null)
                    {
                        accountName = accountName.Substring(0, accountName.Length - 2) + ((int)storageAccountNumber).ToString("D2");
                    }

                    UriBuilder fullUri = new()
                    {
                        Scheme = "https",
                        Host = $"{accountName}.blob.core.windows.net",
                        Path = $"{containerName}"
                    };

                    client = new BlobContainerClient(fullUri.Uri, GetCachedCredentials());
                    _memoryCache.Set(cacheKey, client, _cacheEntryOptionsBlobClient);
                }

                return client;
            }

            StorageSharedKeyCredential storageCredentials = new(_storageConfig.AccountName, _storageConfig.AccountKey);
            Uri storageUrl = new(_storageConfig.BlobEndPoint);
            BlobServiceClient commonBlobClient = new(storageUrl, storageCredentials);
            return commonBlobClient.GetBlobContainerClient(_storageConfig.StorageContainer);
        }

        private TokenCredential GetCachedCredentials()
        {
            if (!_memoryCache.TryGetValue(_credsCacheKey, out DefaultAzureCredential creds))
            {
                creds = new();
                _memoryCache.Set(_credsCacheKey, creds, _cacheEntryOptionsCreds);
            }

            return creds;
        }

        private static string GetClientCacheKey(string org, int? storageAccountNumber)
        {
            return $"blob-{org}-{storageAccountNumber}";
        }
    }
}
