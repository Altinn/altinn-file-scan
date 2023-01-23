using System.Text;
using System.Text.Json;

using Altinn.FileScan.Clients.Interfaces;
using Altinn.FileScan.Configuration;
using Altinn.FileScan.Extensions;
using Altinn.FileScan.Services.Interfaces;
using Altinn.Platform.Storage.Interface.Enums;

using Microsoft.Extensions.Options;

namespace Altinn.FileScan.Clients
{
    /// <summary>
    /// Implementation of the <see cref="IStorageClient"/>
    /// </summary>
    public class StorageClient : IStorageClient
    {
        private readonly HttpClient _client;
        private readonly IAccessToken _accessToken;
        private readonly ILogger<IStorageClient> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageClient"/> class.
        /// </summary>
        public StorageClient(
           HttpClient httpClient,
           IAccessToken accessToken,
           IOptions<PlatformSettings> settings,
           ILogger<IStorageClient> logger)
        {
            _client = httpClient;
            _accessToken = accessToken;

            var platformSettings = settings.Value;
            _client.BaseAddress = new Uri(platformSettings.ApiStorageEndpoint);
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<bool> PatchDataElementFileScanResult(string dataElementId, FileScanResult fileScanResult)
        {
            /* string endpoint = $"dataelement/{dataElementId}/filescan";
             StringContent httpContent = new(JsonSerializer.Serialize(fileScanResult), Encoding.UTF8, "application/json");

             var accessToken = await _accessToken.Generate();

             HttpResponseMessage response = await _client.PostAsync(endpoint, httpContent, accessToken);

             if (!response.IsSuccessStatusCode)
             {
                 return false;
             }
            */
            return true;
        }
    }
}
