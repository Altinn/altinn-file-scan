using System.Text.Json;
using System.Text.Json.Nodes;

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
        private readonly ILogger<IMuescheliClient> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MuescheliClient"/> class.
        /// </summary>
        public MuescheliClient(
           HttpClient httpClient,
           IOptions<PlatformSettings> settings,
           ILogger<IMuescheliClient> logger)
        {
            _client = httpClient;

            var platformSettings = settings.Value;
            _client.BaseAddress = new Uri(platformSettings.ApiClamAvEndpoint);
            _logger = logger;
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

            _logger.LogInformation($"//Muescheli client // Scan stream // Response: {await response.Content.ReadAsStringAsync()}");

            if (!response.IsSuccessStatusCode)
            {
                throw new MuescheliHttpException();
            }

            var jsonContent = await response.Content.ReadAsStringAsync();

            MuescheliResponse r = JsonSerializer.Deserialize<MuescheliResponse>(jsonContent);

            return r.Result;
        }
    }
}
