using System.Text.Json;

using Altinn.FileScan.Clients.Interfaces;
using Altinn.FileScan.Configuration;
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
        public async Task<ScanResult> ScanStream(Stream stream)
        {
            string endpoint = $"scan";
            StreamContent httpContent = new StreamContent(stream);

            HttpResponseMessage response = await _client.PostAsync(endpoint, httpContent);

            _logger.LogInformation($"//Muescheli client // Scan stream // Response: {JsonSerializer.Serialize(response)}");
            _logger.LogInformation($"//Muescheli client // Scan stream // Response: {await response.Content.ReadAsStringAsync()}");

            /*  if (!response.IsSuccessStatusCode)
              {
                  throw new Exception();
              }*/

            // TODO: handle response object
            return ScanResult.OK;
        }
    }
}
