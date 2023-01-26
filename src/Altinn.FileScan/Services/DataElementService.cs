using Altinn.FileScan.Clients.Interfaces;
using Altinn.FileScan.Exceptions;
using Altinn.FileScan.Models;
using Altinn.FileScan.Repository.Interfaces;
using Altinn.FileScan.Services.Interfaces;
using Altinn.Platform.Storage.Interface.Enums;
using Altinn.Platform.Storage.Interface.Models;

namespace Altinn.FileScan.Services
{
    /// <summary>
    /// Implementation of the IDataElement service integrating with Blob Storage and ClamAV complete the scan of a data element.
    /// </summary>
    public class DataElementService : IDataElement
    {
        private readonly IAppOwnerBlob _repository;
        private readonly IStorageClient _storageClient;
        private readonly IMuescheliClient _muescheliClient;
        private readonly ILogger<IDataElement> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataElementService"/> class.
        /// </summary>
        public DataElementService(IAppOwnerBlob repository, IMuescheliClient muescheliClient, IStorageClient storageClient, ILogger<IDataElement> logger)
        {
            _repository = repository;
            _storageClient = storageClient;
            _muescheliClient = muescheliClient;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task Scan(DataElementScanRequest scanRequest)
        {
            try
            {
                var stream = await _repository.GetBlob(scanRequest.Org, scanRequest.BlobStoragePath);

                var filename = string.IsNullOrEmpty(scanRequest.Filename) ? $"{scanRequest.DataElementId}.txt" : scanRequest.Filename;
                ScanResult scanResult = await _muescheliClient.ScanStream(stream, filename);

                FileScanResult fileScanResult = FileScanResult.Pending;

                switch (scanResult)
                {
                    case ScanResult.OK:
                        fileScanResult = FileScanResult.Clean;
                        break;
                    case ScanResult.FOUND:
                        fileScanResult = FileScanResult.Infected;
                        break;
                    case ScanResult.ERROR:
                    case ScanResult.PARSE_ERROR:
                    case ScanResult.UNDEFINED:
                        _logger.LogError("Scan of {dataElementId} completed with unexpected result {scanResult}.", scanRequest.DataElementId, scanResult);
                        throw MuescheliScanResultException.Create(scanRequest.DataElementId, scanResult);
                }

                FileScanStatus status = new()
                {
                    ContentHash = string.Empty,
                    FileScanResult = fileScanResult
                };

                await _storageClient.PatchFileScanStatus(scanRequest.InstanceId, scanRequest.DataElementId, status);
            }
            catch (MuescheliHttpException e)
            {
                _logger.LogError(e, "Scan of {dataElementId} failed with a http exception.", scanRequest.DataElementId);
                throw;
            }
        }
    }
}
