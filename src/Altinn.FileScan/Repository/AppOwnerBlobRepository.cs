using Altinn.FileScan.Models;
using Altinn.FileScan.Repository.Interfaces;
using Azure.Storage.Blobs.Models;

namespace Altinn.FileScan.Repository;

/// <summary>
/// Implementation of IAppOwnerBlob towards Azure Storage
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="AppOwnerBlobRepository"/> class.
/// </remarks>
public class AppOwnerBlobRepository(IBlobContainerClientProvider containerClientProvider) : IAppOwnerBlob
{
    /// <inheritdoc/>
    public async Task<Stream> GetBlob(string org, string blobPath, int? storageAccountNumber)
    {
        var containerClient = containerClientProvider.GetBlobContainerClient(org, storageAccountNumber);
        var blobClient = containerClient.GetBlobClient(blobPath);
        Azure.Response<BlobDownloadInfo> response = await blobClient.DownloadAsync();
        return response.Value.Content;
    }

    /// <inheritdoc/>
    public async Task<BlobPropertyModel> GetBlobProperties(string org, string blobPath, int? storageAccountNumber)
    {
        var containerClient = containerClientProvider.GetBlobContainerClient(org, storageAccountNumber);
        var blobClient = containerClient.GetBlobClient(blobPath);
        Azure.Response<BlobProperties> response = await blobClient.GetPropertiesAsync();
        return new BlobPropertyModel { LastModified = response.Value.LastModified };
    }
}
