using Altinn.FileScan.Clients.Interfaces;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="DataElementService"/> class.
        /// </summary>
        public DataElementService(IAppOwnerBlob repository, IStorageClient storageClient, IMuescheliClient muescheliClient)
        {
            _repository = repository;
            _storageClient = storageClient;
            _muescheliClient = muescheliClient;
        }

        /// <inheritdoc/>
        public async Task<bool> Scan(DataElement dataElement)
        {
            // identify app owner & retrieve access token
            // retrieve blob
            string org = dataElement.BlobStoragePath.Split("/")[0];
            var stream = await _repository.GetBlob(org, dataElement.BlobStoragePath);

            // send for scan            
            ScanResult scanResult = await _muescheliClient.ScanStream(stream);

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
                    return HandleMuesliErrorResult(scanResult);
            }

            await _storageClient.PatchDataElementFileScanResult(dataElement.Id, fileScanResult);

            return true;
        }

        private bool HandleMuesliErrorResult(ScanResult result)
        {
            // throw exception? 
            return false;
        }
    }
}
