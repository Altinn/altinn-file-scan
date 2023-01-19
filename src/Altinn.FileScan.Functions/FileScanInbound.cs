using Altinn.FileScan.Functions.Clients.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Altinn.FileScan.Functions
{
    /// <summary>
    /// Azure Function class.
    /// </summary>
    public class FileScanInbound
    {
        private readonly IFileScanClient _fileScanClient;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileScanInbound"/> class.
        /// </summary>
        /// <param name="fileScanClient">FileScanClient</param>
        /// <param name="loggerFactory">ILoggerFactory</param>
        public FileScanInbound(IFileScanClient fileScanClient, ILoggerFactory loggerFactory)
        {
            _fileScanClient = fileScanClient;
            _logger = loggerFactory.CreateLogger<FileScanInbound>();
        }

        /// <summary>
        /// Retrieves dataElements from file-scna-inbound queue and send to FileScans rest-api
        /// </summary>
        [Function("FileScanInbound")]
        public void Run([QueueTrigger("file-scan-inbound", Connection = "QueueStorage")] string dataElement)
        {
            // Post to the filescan-component using FileScanClient
            _logger.LogInformation($"C# Queue trigger function processed: {dataElement}");
        }
    }
}
