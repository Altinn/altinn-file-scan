using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Altinn.FileScan.Clients.Interfaces;
using Altinn.FileScan.Configuration;
using Altinn.FileScan.Exceptions;
using Altinn.FileScan.Extensions;
using Altinn.FileScan.Models;
using Altinn.FileScan.Services.Interfaces;
using Altinn.Platform.Storage.Interface.Models;
using Microsoft.Extensions.Options;

namespace Altinn.FileScan.Clients;

/// <summary>
/// Implementation of the <see cref="IStorageClient"/>
/// </summary>
public class StorageClient : IStorageClient
{
    /// <summary>
    /// The name of the HTTP client that downloads blobs through the read URLs Storage issues. A read URL grants
    /// access to a blob to anyone who holds it, so this client must not log its requests.
    /// </summary>
    public const string BlobDownloadHttpClientName = "StorageBlobDownload";

    // Storage sends its status enums as strings; a number would mean the contract changed.
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(allowIntegerValues: false) },
    };

    private readonly HttpClient _client;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAccessToken _accessTokenService;

    /// <summary>
    /// Initializes a new instance of the <see cref="StorageClient"/> class.
    /// </summary>
    public StorageClient(
        HttpClient httpClient,
        IHttpClientFactory httpClientFactory,
        IAccessToken accessToken,
        IOptions<PlatformSettings> settings
    )
    {
        _accessTokenService = accessToken;
        _httpClientFactory = httpClientFactory;

        _client = httpClient;
        _client.BaseAddress = new Uri(settings.Value.ApiStorageEndpoint);
    }

    /// <inheritdoc/>
    public async Task<FileScanStatusOutcome> PatchFileScanStatus(
        string instanceId,
        string dataElementId,
        FileScanStatus fileScanStatus
    )
    {
        string endpoint = $"instances/{instanceId}/dataelements/{dataElementId}/filescanstatus";
        StringContent httpContent = new(JsonSerializer.Serialize(fileScanStatus), Encoding.UTF8, "application/json");

        var accessToken = await _accessTokenService.Generate();

        HttpResponseMessage response = await _client.PutAsync(endpoint, httpContent, accessToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new PlatformHttpException(
                response,
                "Unexpected response from StorageClient when setting file scan status."
            );
        }

        StatusResponse<FileScanStatusOutcome> body = await ReadResponse<StatusResponse<FileScanStatusOutcome>>(
            response
        );
        return body.Status;
    }

    /// <inheritdoc/>
    public async Task<BlobToScan> GetBlobToScan(string instanceId, string dataElementId, string blobVersionId)
    {
        string endpoint =
            $"instances/{instanceId}/dataelements/{dataElementId}/blobversions/{Uri.EscapeDataString(blobVersionId)}/filescan";
        string accessToken = await _accessTokenService.Generate();

        HttpResponseMessage response = await _client.GetAsync(endpoint, accessToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new PlatformHttpException(
                response,
                "Unexpected response from StorageClient when getting the file scan target."
            );
        }

        FileScanTargetResponse target = await ReadResponse<FileScanTargetResponse>(response);
        if (target.Status != FileScanTargetStatus.Pending)
        {
            return BlobToScan.NotAwaitingScan(target.Status);
        }

        if (target.ReadUrl is null)
        {
            throw new PlatformHttpException(
                response,
                "Storage returned a pending file scan target without a read URL."
            );
        }

        // The read URL needs no credential of ours, and works for any storage provider: one plain GET gives both
        // the content and Last-Modified, since a signed URL may grant GET only.
        HttpResponseMessage blobResponse = await _httpClientFactory
            .CreateClient(BlobDownloadHttpClientName)
            .GetAsync(target.ReadUrl, HttpCompletionOption.ResponseHeadersRead);
        if (!blobResponse.IsSuccessStatusCode)
        {
            using (blobResponse)
            {
                throw new PlatformHttpException(
                    blobResponse,
                    $"Unexpected response {(int)blobResponse.StatusCode} when downloading the blob to scan."
                );
            }
        }

        Stream content = await blobResponse.Content.ReadAsStreamAsync();
        return BlobToScan.Pending(content, blobResponse);
    }

    private static async Task<T> ReadResponse<T>(HttpResponseMessage response)
    {
        string responseBody = await response.Content.ReadAsStringAsync();
        try
        {
            return JsonSerializer.Deserialize<T>(responseBody, _jsonOptions)
                ?? throw new PlatformHttpException(response, "Storage returned an empty response.");
        }
        catch (JsonException exception)
        {
            throw new PlatformHttpException(response, $"Storage returned an unexpected response: {exception.Message}");
        }
    }

    private sealed record StatusResponse<TStatus>([property: JsonRequired] TStatus Status);

    private sealed record FileScanTargetResponse(
        [property: JsonRequired] FileScanTargetStatus Status,
        Uri? ReadUrl,
        DateTimeOffset? ExpiresAt
    );
}
