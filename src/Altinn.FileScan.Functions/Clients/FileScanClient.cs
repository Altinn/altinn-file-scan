﻿using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Altinn.Common.AccessTokenClient.Services;
using Altinn.FileScan.Functions.Clients.Interfaces;
using Altinn.FileScan.Functions.Configuration;
using Altinn.FileScan.Functions.Extensions;
using Altinn.FileScan.Functions.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Altinn.FileScan.Functions.Clients
{
    /// <inheritdoc/>
    public class FileScanClient : IFileScanClient
    {
        private readonly HttpClient _client;
        private readonly IAccessTokenGenerator _accessTokenGenerator;
        private readonly IKeyVaultService _keyVaultService; 
        private readonly KeyVaultSettings _keyVaultSettings;
        private readonly ILogger<IFileScanClient> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileScanClient"/> class.
        /// </summary>
        public FileScanClient(
            HttpClient httpClient,
            IAccessTokenGenerator accessTokenGenerator,
            IKeyVaultService keyVaultService,
            IOptions<PlatformSettings> platformSettings,
            IOptions<KeyVaultSettings> keyVaultSettings,
            ILogger<IFileScanClient> logger)
        {
            _client = httpClient;
            _accessTokenGenerator = accessTokenGenerator;
            _keyVaultService = keyVaultService;
            _keyVaultSettings = keyVaultSettings.Value;
            _logger = logger;
            _client.BaseAddress = new Uri(platformSettings.Value.ApiFileScanEndpoint);
        }

        /// <inheritdoc/>
        public async Task PostDataElementScanRequest(string dataElementScanRequest)
        {
            StringContent httpContent = new(dataElementScanRequest, Encoding.UTF8, "application/json");

            string endpointUrl = "dataelement";

            var accessToken = await GenerateAccessToken();

            HttpResponseMessage response = await _client.PostAsync(endpointUrl, httpContent, accessToken);
            if (!response.IsSuccessStatusCode)
            {
                var n = JsonNode.Parse(dataElementScanRequest, new() { PropertyNameCaseInsensitive = true });
                string dataElementId = n["dataElementId"].ToString();
                var msg = $"// Post to FileScan for id {dataElementId} failed with status code {response.StatusCode}";
                _logger.LogError("{msg}", msg);
                throw new HttpRequestException(msg);
            }
        }

        /// <summary>
        /// Generate a fresh access token using the client certificate
        /// </summary>
        protected async Task<string> GenerateAccessToken()
        {
            string certBase64 =
            await _keyVaultService.GetCertificateAsync(
                    _keyVaultSettings.KeyVaultURI,
                    _keyVaultSettings.PlatformCertSecretId);
            string accessToken = _accessTokenGenerator.GenerateAccessToken("platform", "file-scan", new X509Certificate2(
                Convert.FromBase64String(certBase64),
                (string)null,
                X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable));
            return accessToken;
        }
    }
}
