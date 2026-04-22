using System.Text;
using System.Text.Json;
using Altinn.FileScan.Clients.Interfaces;
using Altinn.FileScan.Configuration;
using Altinn.FileScan.Exceptions;
using Altinn.FileScan.Extensions;
using Altinn.FileScan.Services.Interfaces;
using Altinn.Platform.Storage.Interface.Models;
using Microsoft.Extensions.Options;

namespace Altinn.FileScan.Clients;

/// <summary>
/// Implementation of the <see cref="IStorageClient"/>
/// </summary>
public class StorageClient(HttpClient httpClient, IAccessToken accessToken, IOptions<PlatformSettings> settings) : IStorageClient
{
    private readonly HttpClient _client = Configure(httpClient, settings.Value.ApiStorageEndpoint);
    private readonly IAccessToken _accessTokenService = accessToken;

    /// <inheritdoc/>
    public async Task PatchFileScanStatus(string instanceId, string dataElementId, FileScanStatus fileScanStatus)
    {
        string endpoint = $"instances/{instanceId}/dataelements/{dataElementId}/filescanstatus";
        StringContent httpContent = new(JsonSerializer.Serialize(fileScanStatus), Encoding.UTF8, "application/json");

        var accessToken = await _accessTokenService.Generate();

        HttpResponseMessage response = await _client.PutAsync(endpoint, httpContent, accessToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new PlatformHttpException(response, "Unexpected response from StorageClient when setting file scan status.");
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DataElementExists(string instanceId, string dataElementId)
    {
        string endpoint = $"instances/{instanceId}/dataelementexists/{dataElementId}";
        string accessToken = await _accessTokenService.Generate();

        HttpResponseMessage response = await _client.GetAsync(endpoint, accessToken);

        if (response.IsSuccessStatusCode)
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            if (bool.TryParse(responseBody, out bool result))
            {
                return result;
            }
        }

        throw new PlatformHttpException(response, "Unexpected response from StorageClient when checking if data element exists.");
    }

    private static HttpClient Configure(HttpClient client, string endpoint)
    {
        client.BaseAddress = new Uri(endpoint);
        return client;
    }
}
