﻿using System.Text;
using System.Text.Json;

using Altinn.FileScan.Clients.Interfaces;
using Altinn.FileScan.Configuration;
using Altinn.FileScan.Extensions;
using Altinn.FileScan.Services.Interfaces;
using Altinn.Platform.Storage.Interface.Models;

using Microsoft.Extensions.Options;

namespace Altinn.FileScan.Clients
{
    /// <summary>
    /// Implementation of the <see cref="IStorageClient"/>
    /// </summary>
    public class StorageClient : IStorageClient
    {
        private readonly HttpClient _client;
        private readonly IAccessToken _accessTokenService;
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
            _accessTokenService = accessToken;

            _client = httpClient;
            _client.BaseAddress = new Uri(settings.Value.ApiStorageEndpoint);

            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task PatchFileScanStatus(string instanceId, string dataElementId, FileScanStatus fileScanStatus)
        {
            string endpoint = $"instances/{instanceId}/dataelements/{dataElementId}/filescanstatus";
            StringContent httpContent = new(JsonSerializer.Serialize(fileScanStatus), Encoding.UTF8, "application/json");

            var accessToken = await _accessTokenService.Generate();

            _logger.LogInformation($"Access token sent to storage {accessToken}");

            HttpResponseMessage response = await _client.PutAsync(endpoint, httpContent, accessToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Unexpected response from storage {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            }
        }
    }
}
