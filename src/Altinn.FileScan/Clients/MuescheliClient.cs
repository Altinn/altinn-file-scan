using System.Text.Json;
using System.Text.Json.Serialization;

using Altinn.FileScan.Clients.Interfaces;
using Altinn.FileScan.Configuration;
using Altinn.FileScan.Exceptions;
using Altinn.FileScan.Models;

using Microsoft.Extensions.Options;

namespace Altinn.FileScan.Clients
{
    /// <summary>
    /// Implementation of the <see cref="IMuescheliClient"/>
    /// </summary>
    public class MuescheliClient : IMuescheliClient
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _serializerOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="MuescheliClient"/> class.
        /// </summary>
        public MuescheliClient(
           HttpClient httpClient,
           IOptions<PlatformSettings> settings)
        {
            _client = httpClient;
            _client.BaseAddress = new Uri(settings.Value.ApiMuescheliEndpoint);

            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };
        }

        /// <inheritdoc/>
        public async Task<ScanResult> ScanStream(Stream stream, string filename)
        {
            string endpoint = $"scan";

            using var content = new MultipartFormDataContent
            {
                { new StreamContent(stream), "file", filename }
            };

            HttpResponseMessage response = await _client.PostAsync(endpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                 throw await MuescheliHttpException.CreateAsync(response.StatusCode, response);
            }

            var responseString = await response.Content.ReadAsStringAsync();

            MuescheliResponse r = JsonSerializer.Deserialize<List<MuescheliResponse>>(responseString, _serializerOptions)
                .First();

            return r.Result;
        }
    }
}
