#nullable disable

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Altinn.FileScan.Clients.Interfaces;
using Altinn.FileScan.Exceptions;
using Altinn.FileScan.Models;
using Altinn.FileScan.Services;
using Altinn.Platform.Storage.Interface.Enums;
using Altinn.Platform.Storage.Interface.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Altinn.FileScan.Tests.TestingServices;

public class DataElementServiceTests
{
    private readonly DataElementScanRequest _defaultDataElementScanRequest = new()
    {
        DataElementId = "dataElementId",
        InstanceId = "instanceId",
        Filename = "attachment.pdf",
        BlobVersionId = "blobVersionId",
    };

    /// <summary>
    /// Scenario: The blob version awaits its scan.
    /// Expected result: The blob from Storage is scanned and the result is sent to Storage.
    /// Success criteria: Exactly the blob's content is scanned, the result is sent with the blob version, and the
    /// blob is disposed afterwards.
    /// </summary>
    [Fact]
    public async Task Scan_HappyPath_AllServicesAreCalledWithExpectedData()
    {
        // Arrange
        MemoryStream content = new([1, 2, 3]);
        Mock<IStorageClient> storageClientMock = new();
        storageClientMock
            .Setup(s => s.GetBlobToScan("instanceId", "dataElementId", "blobVersionId"))
            .ReturnsAsync(BlobToScan.Pending(content));
        storageClientMock
            .Setup(s =>
                s.PatchFileScanStatus(
                    "instanceId",
                    "dataElementId",
                    It.Is<FileScanStatus>(f =>
                        f.FileScanResult == FileScanResult.Clean && f.BlobVersionId == "blobVersionId"
                    )
                )
            )
            .ReturnsAsync(FileScanStatusOutcome.Updated);

        Mock<IMuescheliClient> muescheliClientMock = new();
        muescheliClientMock.Setup(m => m.ScanStream(content, It.IsAny<string>())).ReturnsAsync(ScanResult.OK);

        DataElementService sut = SetUpTestService(muescheliClientMock.Object, storageClientMock.Object);

        // Act
        await sut.Scan(_defaultDataElementScanRequest);

        // Assert
        muescheliClientMock.VerifyAll();
        storageClientMock.VerifyAll();
        Assert.False(content.CanRead);
    }

    [Fact]
    public async Task Scan_FilenameMissing_DataElementIdIsUsed()
    {
        // Arrange
        Mock<IMuescheliClient> muescheliClientMock = new();
        muescheliClientMock
            .Setup(m => m.ScanStream(It.IsAny<Stream>(), It.Is<string>(s => s == "dataElementId.bin")))
            .ReturnsAsync(ScanResult.OK);

        DataElementService sut = SetUpTestService(muescheliClient: muescheliClientMock.Object);

        var input = new DataElementScanRequest
        {
            DataElementId = "dataElementId",
            InstanceId = "instanceId",
            BlobVersionId = "blobVersionId",
        };

        // Act
        await sut.Scan(input);

        // Assert
        muescheliClientMock.VerifyAll();
    }

    [Theory]
    [InlineData(ScanResult.OK, FileScanResult.Clean)]
    [InlineData(ScanResult.FOUND, FileScanResult.Infected)]
    public async Task Scan_DeterminateScanResult_MappedCorrectlyToFileSCcanResult(
        ScanResult scanResult,
        FileScanResult expectedFileScanResult
    )
    {
        // Arrange
        Mock<IMuescheliClient> muescheliClientMock = new();
        muescheliClientMock.Setup(m => m.ScanStream(It.IsAny<Stream>(), It.IsAny<string>())).ReturnsAsync(scanResult);

        Mock<IStorageClient> storageClientMock = StorageClientWithBlob();
        storageClientMock
            .Setup(s =>
                s.PatchFileScanStatus(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.Is<FileScanStatus>(f => f.FileScanResult == expectedFileScanResult)
                )
            )
            .ReturnsAsync(FileScanStatusOutcome.Updated);

        DataElementService sut = SetUpTestService(muescheliClientMock.Object, storageClientMock.Object);

        // Act
        await sut.Scan(_defaultDataElementScanRequest);

        // Assert
        muescheliClientMock.VerifyAll();
        storageClientMock.VerifyAll();
    }

    [Theory]
    [InlineData(ScanResult.UNDEFINED)]
    [InlineData(ScanResult.ERROR)]
    [InlineData(ScanResult.PARSE_ERROR)]
    public async Task Scan_IndeterminateScanResult_ExceptionIsThrown(ScanResult scanResult)
    {
        // Arrange
        Mock<IMuescheliClient> muescheliClientMock = new();
        muescheliClientMock.Setup(m => m.ScanStream(It.IsAny<Stream>(), It.IsAny<string>())).ReturnsAsync(scanResult);

        DataElementService sut = SetUpTestService(muescheliClient: muescheliClientMock.Object);

        // Act
        Task Act() => sut.Scan(_defaultDataElementScanRequest);

        // Assert
        MuescheliScanResultException exception = await Assert.ThrowsAsync<MuescheliScanResultException>(Act);
        Assert.Equal(
            $"Muescheli scan returned result code `{scanResult}` for data element with id dataElementId",
            exception.Message
        );
    }

    [Fact]
    public async Task Scan_MuescliClientThrowsException_ExceptionBubblesUp()
    {
        // Arrange
        Mock<IMuescheliClient> muescheliClientMock = new();
        muescheliClientMock
            .Setup(m => m.ScanStream(It.IsAny<Stream>(), It.IsAny<string>()))
            .ThrowsAsync(
                await MuescheliHttpException.CreateAsync(
                    HttpStatusCode.NotFound,
                    new HttpResponseMessage(HttpStatusCode.NotFound)
                )
            );

        DataElementService sut = SetUpTestService(muescheliClient: muescheliClientMock.Object);

        // Act
        Task Act() => sut.Scan(_defaultDataElementScanRequest);

        // Assert
        await Assert.ThrowsAsync<MuescheliHttpException>(Act);
    }

    /// <summary>
    /// Scenario: The scan request names no blob version.
    /// Expected result: The blob can't be found, so the scan is skipped.
    /// Success criteria: An error is logged and nothing is requested from Storage.
    /// </summary>
    [Fact]
    public async Task Scan_NoBlobVersionId_ScanSkipped_ErrorLogged()
    {
        // Arrange
        Mock<IStorageClient> storageClient = new(MockBehavior.Strict);
        Mock<ILogger<DataElementService>> loggerMock = new();

        DataElementService sut = SetUpTestService(storageClient: storageClient.Object, logger: loggerMock.Object);

        var input = new DataElementScanRequest { DataElementId = "dataElementId", InstanceId = "instanceId" };

        // Act
        await sut.Scan(input);

        // Assert
        VerifyLogged(loggerMock, LogLevel.Error, Times.Once());
    }

    /// <summary>
    /// Scenario: Storage says the blob version no longer awaits its scan.
    /// Expected result: The scan is skipped; this is normal, not an error.
    /// Success criteria: Nothing is scanned, no status is sent, and the reason is logged as information.
    /// </summary>
    [Theory]
    [InlineData(FileScanTargetStatus.Superseded)]
    [InlineData(FileScanTargetStatus.NotPending)]
    [InlineData(FileScanTargetStatus.Deleted)]
    public async Task Scan_NotAwaitingScan_ScanSkipped(FileScanTargetStatus status)
    {
        // Arrange
        Mock<IStorageClient> storageClient = new();
        storageClient
            .Setup(s => s.GetBlobToScan(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(BlobToScan.NotAwaitingScan(status));
        Mock<IMuescheliClient> muescheliClient = new(MockBehavior.Strict);
        Mock<ILogger<DataElementService>> loggerMock = new();

        DataElementService sut = SetUpTestService(muescheliClient.Object, storageClient.Object, loggerMock.Object);

        // Act
        await sut.Scan(_defaultDataElementScanRequest);

        // Assert
        storageClient.Verify(
            s => s.PatchFileScanStatus(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<FileScanStatus>()),
            Times.Never
        );
        VerifyLogged(loggerMock, LogLevel.Information, Times.Once());
        VerifyLogged(loggerMock, LogLevel.Warning, Times.Never());
        VerifyLogged(loggerMock, LogLevel.Error, Times.Never());
    }

    /// <summary>
    /// Scenario: Storage doesn't decide, or the blob can't be downloaded.
    /// Expected result: The request fails, so it is retried and Storage decides again.
    /// Success criteria: The exception bubbles up and nothing is scanned.
    /// </summary>
    [Fact]
    public async Task Scan_GettingBlobFails_ExceptionBubblesUp()
    {
        // Arrange
        Mock<IStorageClient> storageClient = new();
        storageClient
            .Setup(s => s.GetBlobToScan(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new PlatformHttpException(new HttpResponseMessage(HttpStatusCode.NotFound), "404"));
        Mock<IMuescheliClient> muescheliClient = new(MockBehavior.Strict);

        DataElementService sut = SetUpTestService(muescheliClient.Object, storageClient.Object);

        // Act
        Task Act() => sut.Scan(_defaultDataElementScanRequest);

        // Assert
        PlatformHttpException exception = await Assert.ThrowsAsync<PlatformHttpException>(Act);
        Assert.Equal(HttpStatusCode.NotFound, exception.Response.StatusCode);
    }

    /// <summary>
    /// Scenario: Storage doesn't set the scan result, because a newer version replaced the scanned one or the
    /// data element is gone.
    /// Expected result: This is normal, not an error.
    /// Success criteria: The scan completes, and the outcome is logged as information.
    /// </summary>
    [Theory]
    [InlineData(FileScanStatusOutcome.Superseded)]
    [InlineData(FileScanStatusOutcome.Deleted)]
    public async Task Scan_ResultNotSet_LogsOutcome(FileScanStatusOutcome outcome)
    {
        // Arrange
        Mock<IStorageClient> storageClient = StorageClientWithBlob();
        storageClient
            .Setup(s => s.PatchFileScanStatus(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<FileScanStatus>()))
            .ReturnsAsync(outcome);
        Mock<ILogger<DataElementService>> loggerMock = new();

        DataElementService sut = SetUpTestService(storageClient: storageClient.Object, logger: loggerMock.Object);

        // Act
        await sut.Scan(_defaultDataElementScanRequest);

        // Assert
        VerifyLogged(loggerMock, LogLevel.Information, Times.Once());
        VerifyLogged(loggerMock, LogLevel.Error, Times.Never());
    }

    private static void VerifyLogged(Mock<ILogger<DataElementService>> loggerMock, LogLevel level, Times times)
    {
        loggerMock.Verify(
            x =>
                x.Log(
                    level,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
                ),
            times
        );
    }

    private static Mock<IStorageClient> StorageClientWithBlob()
    {
        Mock<IStorageClient> storageClientMock = new();
        storageClientMock
            .Setup(s => s.GetBlobToScan(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(() => BlobToScan.Pending(new MemoryStream([1, 2, 3])));
        return storageClientMock;
    }

    private static DataElementService SetUpTestService(
        IMuescheliClient muescheliClient = null,
        IStorageClient storageClient = null,
        ILogger<DataElementService> logger = null
    )
    {
        if (muescheliClient is null)
        {
            Mock<IMuescheliClient> muescheliClientMock = new();
            muescheliClientMock
                .Setup(m => m.ScanStream(It.IsAny<Stream>(), It.IsAny<string>()))
                .ReturnsAsync(ScanResult.OK);

            muescheliClient = muescheliClientMock.Object;
        }

        if (storageClient is null)
        {
            Mock<IStorageClient> storageClientMock = StorageClientWithBlob();
            storageClientMock
                .Setup(s => s.PatchFileScanStatus(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<FileScanStatus>()))
                .ReturnsAsync(FileScanStatusOutcome.Updated);

            storageClient = storageClientMock.Object;
        }

        logger ??= new Mock<ILogger<DataElementService>>().Object;

        return new DataElementService(muescheliClient, storageClient, logger);
    }
}
