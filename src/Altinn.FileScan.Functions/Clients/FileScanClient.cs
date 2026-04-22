#nullable disable

using System;
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

namespace Altinn.FileScan.Functions.Clients;

/// <inheritdoc/>
public class FileScanClient(
    HttpClient httpClient,
    IAccessTokenGenerator accessTokenGenerator,
    ICertificateResolverService certificateResolverService,
    IOptions<PlatformSettings> platformSettings,
    ILogger<FileScanClient> logger) : IFileScanClient
{
    private readonly HttpClient _client = Configure(httpClient, platformSettings.Value.ApiFileScanEndpoint);

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
            logger.LogError("{msg}", msg);
            throw new HttpRequestException(msg);
        }
    }

    /// <summary>
    /// Generate a fresh access token using the client certificate
    /// </summary>
    protected async Task<string> GenerateAccessToken()
    {
        X509Certificate2 certificate = await certificateResolverService.GetCertificateAsync();

        string accessToken = accessTokenGenerator.GenerateAccessToken("platform", "file-scan", certificate);
        return accessToken;
    }

    private static HttpClient Configure(HttpClient client, string endpoint)
    {
        client.BaseAddress = new Uri(endpoint);
        return client;
    }
}
