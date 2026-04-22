using Altinn.FileScan.Clients.Interfaces;
using Altinn.FileScan.Exceptions;
using Altinn.FileScan.Models;
using Altinn.FileScan.Repository.Interfaces;
using Altinn.FileScan.Services.Interfaces;
using Altinn.Platform.Storage.Interface.Enums;
using Altinn.Platform.Storage.Interface.Models;
using Azure;

namespace Altinn.FileScan.Services;

/// <summary>
/// Implementation of the IDataElement service integrating with Blob Storage and ClamAV complete the scan of a data element.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DataElementService"/> class.
/// </remarks>
public class DataElementService(IAppOwnerBlob repository, IMuescheliClient muescheliClient, IStorageClient storageClient, ILogger<DataElementService> logger) : IDataElement
{
    /// <inheritdoc/>
    public async Task Scan(DataElementScanRequest scanRequest)
    {
        try
        {
            BlobPropertyModel? blobProps = null;
            try
            {
                blobProps = await repository.GetBlobProperties(scanRequest.Org, scanRequest.BlobStoragePath, scanRequest.StorageAccountNumber);
            }
            catch (RequestFailedException exception)
            {
                logger.LogWarning(
                    exception,
                    "Blob not found with the following parameters. Org: {Org}, BlobStoragePath: {BlobStoragePath}, StorageAccountNumber: {StorageAccountNumber}",
                    scanRequest.Org.Replace(Environment.NewLine, string.Empty),
                    scanRequest.BlobStoragePath.Replace(Environment.NewLine, string.Empty),
                    scanRequest.StorageAccountNumber);

                bool result = await storageClient.DataElementExists(scanRequest.InstanceId, scanRequest.DataElementId);
                if (result)
                {
                    RequestFailedException requestFailedException = new($"DataElement {scanRequest.DataElementId} found and blob does not exist");
                    logger.LogError(
                        requestFailedException,
                        "DataElement {DataElementId} found and blob does not exist",
                        scanRequest.DataElementId.Replace(Environment.NewLine, string.Empty));
                    throw requestFailedException;
                }

                logger.LogWarning("DataElement and Blob not found, scan skipped.");
                return;
            }

            if (blobProps?.LastModified != scanRequest.Timestamp)
            {
                // we replace newline characters in log messages to avoid log injection attacks
                double totalSeconds = blobProps is not null ? scanRequest.Timestamp.Subtract(blobProps.LastModified).TotalSeconds : 0;
                logger.LogError(
                    "Scan request timestamp != blob last modified timestamp, scan request aborted. Instance Id: {InstanceId}, DataElementId: {DataElementId}, timestamp diff: {TimeDiff} seconds",
                    scanRequest.InstanceId.Replace(Environment.NewLine, string.Empty),
                    scanRequest.DataElementId.Replace(Environment.NewLine, string.Empty),
                    totalSeconds);
                return;
            }

            var stream = await repository.GetBlob(scanRequest.Org, scanRequest.BlobStoragePath, scanRequest.StorageAccountNumber);

            var filename = $"{scanRequest.DataElementId}.bin";
            ScanResult scanResult = await muescheliClient.ScanStream(stream, filename);

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
                    logger.LogError("Scan of {DataElementId} completed with unexpected result {ScanResult}.", scanRequest.DataElementId.Replace(Environment.NewLine, string.Empty), scanResult);
                    throw MuescheliScanResultException.Create(scanRequest.DataElementId, scanResult);
            }

            FileScanStatus status = new()
            {
                ContentHash = string.Empty,
                FileScanResult = fileScanResult
            };

            await storageClient.PatchFileScanStatus(scanRequest.InstanceId, scanRequest.DataElementId, status);
        }
        catch (MuescheliHttpException e)
        {
            logger.LogError(e, "Scan of {DataElementId} failed with an http exception.", scanRequest.DataElementId.Replace(Environment.NewLine, string.Empty));
            throw;
        }
    }
}
