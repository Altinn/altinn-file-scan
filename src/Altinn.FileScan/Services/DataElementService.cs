using System;
using System.Threading.Tasks;
using Altinn.FileScan.Clients.Interfaces;
using Altinn.FileScan.Exceptions;
using Altinn.FileScan.Models;
using Altinn.FileScan.Services.Interfaces;
using Altinn.Platform.Storage.Interface.Enums;
using Altinn.Platform.Storage.Interface.Models;
using Microsoft.Extensions.Logging;

namespace Altinn.FileScan.Services;

/// <summary>
/// Implementation of the IDataElement service, which gets a data element's blob from Altinn Storage and scans it
/// with ClamAV.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DataElementService"/> class.
/// </remarks>
public class DataElementService(
    IMuescheliClient muescheliClient,
    IStorageClient storageClient,
    ILogger<DataElementService> logger
) : IDataElement
{
    private readonly IStorageClient _storageClient = storageClient;
    private readonly IMuescheliClient _muescheliClient = muescheliClient;
    private readonly ILogger _logger = logger;

    /// <inheritdoc/>
    public async Task Scan(DataElementScanRequest scanRequest)
    {
        try
        {
            if (string.IsNullOrEmpty(scanRequest.BlobVersionId))
            {
                _logger.LogError(
                    "Scan request for DataElement {DataElementId} names no blob version, scan skipped.",
                    scanRequest.DataElementId.Replace(Environment.NewLine, string.Empty)
                );
                return;
            }

            using BlobToScan blob = await _storageClient.GetBlobToScan(
                scanRequest.InstanceId,
                scanRequest.DataElementId,
                scanRequest.BlobVersionId
            );
            if (blob.Status != FileScanTargetStatus.Pending)
            {
                _logger.LogInformation(
                    "Blob version {BlobVersionId} of DataElement {DataElementId} does not await a scan ({Status}), scan skipped.",
                    scanRequest.BlobVersionId.Replace(Environment.NewLine, string.Empty),
                    scanRequest.DataElementId.Replace(Environment.NewLine, string.Empty),
                    blob.Status
                );
                return;
            }

            var filename = $"{scanRequest.DataElementId}.bin";
            ScanResult scanResult = await _muescheliClient.ScanStream(blob.Content!, filename);

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
                    _logger.LogError(
                        "Scan of {DataElementId} completed with unexpected result {ScanResult}.",
                        scanRequest.DataElementId.Replace(Environment.NewLine, string.Empty),
                        scanResult
                    );
                    throw MuescheliScanResultException.Create(scanRequest.DataElementId, scanResult);
            }

            // With the blob version, Storage ignores the result if a newer version has replaced this one, whose
            // own scan reports for it.
            FileScanStatus status = new()
            {
                ContentHash = string.Empty,
                FileScanResult = fileScanResult,
                BlobVersionId = scanRequest.BlobVersionId,
            };

            FileScanStatusOutcome outcome = await _storageClient.PatchFileScanStatus(
                scanRequest.InstanceId,
                scanRequest.DataElementId,
                status
            );
            if (outcome != FileScanStatusOutcome.Updated)
            {
                _logger.LogInformation(
                    "Scan result for blob version {BlobVersionId} of DataElement {DataElementId} was not set ({Outcome}).",
                    scanRequest.BlobVersionId.Replace(Environment.NewLine, string.Empty),
                    scanRequest.DataElementId.Replace(Environment.NewLine, string.Empty),
                    outcome
                );
            }
        }
        catch (MuescheliHttpException e)
        {
            _logger.LogError(
                e,
                "Scan of {DataElementId} failed with an http exception.",
                scanRequest.DataElementId.Replace(Environment.NewLine, string.Empty)
            );
            throw;
        }
    }
}
