#nullable disable

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Altinn.FileScan.Clients.Interfaces;
using Altinn.FileScan.Exceptions;
using Altinn.FileScan.Models;
using Altinn.FileScan.Repository.Interfaces;
using Altinn.FileScan.Services;
using Altinn.Platform.Storage.Interface.Enums;
using Altinn.Platform.Storage.Interface.Models;
using Azure;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Altinn.FileScan.Tests.TestingServices
{
    public class DataElementServiceTests
    {
        private static readonly DateTimeOffset _requestTimestamp = new(2023, 1, 10, 8, 0, 0, new TimeSpan(0, 0, 0));
        private static readonly DateTimeOffset _matchingTimestamp = new(2023, 1, 10, 8, 0, 0, new TimeSpan(0, 0, 0));
        private static readonly DateTimeOffset _nonMatchingTimestamp = new(2023, 1, 10, 8, 30, 0, new TimeSpan(0, 0, 0));

        private readonly DataElementScanRequest _defaultDataElementScanRequest = new()
        {
            BlobStoragePath = "blobstoragePath/org/attachment.pdf",
            DataElementId = "dataElementId",
            InstanceId = "instanceId",
            Timestamp = _requestTimestamp,
            Filename = "attachment.pdf",
            Org = "ttd"
        };

        [Fact]
        public async Task Scan_HappyPath_AllServicesAreCalledWithExpectedData()
        {
            // Arrange
            Mock<IAppOwnerBlob> blobMock = new();
            blobMock
                .Setup(b => b.GetBlobProperties(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync(new BlobPropertyModel
                {
                    LastModified = _matchingTimestamp
                });
            blobMock
                .Setup(b => b.GetBlob(It.Is<string>(s => s == "ttd"), It.Is<string>(s => s == "blobstoragePath/org/attachment.pdf"), null))
                .ReturnsAsync((Stream)null);

            Mock<IMuescheliClient> muescheliClientMock = new();
            muescheliClientMock.Setup(m => m.ScanStream(It.IsAny<Stream>(), It.IsAny<string>()))
                .ReturnsAsync(ScanResult.OK);

            Mock<IStorageClient> storageClientMock = new();
            storageClientMock
                .Setup(s => s.PatchFileScanStatus(It.Is<string>(s => s == "instanceId"), It.Is<string>(s => s == "dataElementId"), It.Is<FileScanStatus>(f => f.FileScanResult == FileScanResult.Clean)));

            DataElementService sut = SetUpTestService(blobMock.Object, muescheliClientMock.Object, storageClientMock.Object);

            // Act
            await sut.Scan(_defaultDataElementScanRequest);

            // Assert
            blobMock.VerifyAll();
            muescheliClientMock.VerifyAll();
            storageClientMock.VerifyAll();
        }

        [Fact]
        public async Task Scan_FilenameMissing_DataElementIdIsUsed()
        {
            // Arrange
            Mock<IMuescheliClient> muescheliClientMock = new();
            muescheliClientMock.Setup(m => m.ScanStream(It.IsAny<Stream>(), It.Is<string>(s => s == "dataElementId.bin")))
                .ReturnsAsync(ScanResult.OK);

            DataElementService sut = SetUpTestService(muescheliClient: muescheliClientMock.Object);

            var input = new DataElementScanRequest
            {
                BlobStoragePath = "blobstoragePath/org/attachment.pdf",
                DataElementId = "dataElementId",
                Timestamp = _requestTimestamp,
                InstanceId = "instanceId",
                Org = "ttd"
            };

            // Act
            await sut.Scan(input);

            // Assert
            muescheliClientMock.VerifyAll();
        }

        [Fact]
        public async Task Scan_TimestampDoesNotMatch_ErrorLogged()
        {
            // Arrange
            Mock<IAppOwnerBlob> blobMock = new();
            blobMock
                .Setup(b => b.GetBlobProperties(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync(new BlobPropertyModel { LastModified = _nonMatchingTimestamp });
            blobMock
                .Setup(b => b.GetBlob(It.IsAny<string>(), It.IsAny<string>(), null))
                .ReturnsAsync((Stream)null);

            Mock<ILogger<DataElementService>> loggerMock = new Mock<ILogger<DataElementService>>();

            Mock<IMuescheliClient> muescheliClientMock = new();
            muescheliClientMock.Setup(m => m.ScanStream(It.IsAny<Stream>(), It.Is<string>(s => s == "dataElementId.txt")))
                .ReturnsAsync(ScanResult.OK);

            DataElementService sut = SetUpTestService(
                appOwnerBlob: blobMock.Object,
                muescheliClient: muescheliClientMock.Object,
                logger: loggerMock.Object);

            var input = new DataElementScanRequest
            {
                BlobStoragePath = "blobstoragePath/org/attachment.pdf",
                DataElementId = "dataElementId",
                Timestamp = _requestTimestamp,
                InstanceId = "instanceId",
                Org = "ttd"
            };

            // Act
            await sut.Scan(input);

            // Assert
            loggerMock.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
            muescheliClientMock.Verify(x => x.ScanStream(It.IsAny<Stream>(), It.IsAny<string>()), Times.Never);
        }

        [Theory]
        [InlineData(ScanResult.OK, FileScanResult.Clean)]
        [InlineData(ScanResult.FOUND, FileScanResult.Infected)]
        public async Task Scan_DeterminateScanResult_MappedCorrectlyToFileSCcanResult(ScanResult scanResult, FileScanResult expectedFileScanResult)
        {
            // Arrange
            Mock<IMuescheliClient> muescheliClientMock = new();
            muescheliClientMock.Setup(m => m.ScanStream(It.IsAny<Stream>(), It.IsAny<string>()))
                .ReturnsAsync(scanResult);

            Mock<IStorageClient> storageClientMock = new();
            storageClientMock
                .Setup(s => s.PatchFileScanStatus(It.IsAny<string>(), It.IsAny<string>(), It.Is<FileScanStatus>(f => f.FileScanResult == expectedFileScanResult)));

            DataElementService sut = SetUpTestService(muescheliClient: muescheliClientMock.Object, storageClient: storageClientMock.Object);

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
            muescheliClientMock.Setup(m => m.ScanStream(It.IsAny<Stream>(), It.IsAny<string>()))
                .ReturnsAsync(scanResult);

            DataElementService sut = SetUpTestService(muescheliClient: muescheliClientMock.Object);

            // Act
            Task Act() => sut.Scan(_defaultDataElementScanRequest);

            // Assert
            MuescheliScanResultException exception = await Assert.ThrowsAsync<MuescheliScanResultException>(Act);
            Assert.Equal($"Muescheli scan returned result code `{scanResult}` for data element with id dataElementId", exception.Message);
        }

        [Fact]
        public async Task Scan_MuescliClientThrowsException_ExceptionBubblesUp()
        {
            // Arrange
            Mock<IMuescheliClient> muescheliClientMock = new();
            muescheliClientMock.Setup(m => m.ScanStream(It.IsAny<Stream>(), It.IsAny<string>()))
                .ThrowsAsync(await MuescheliHttpException.CreateAsync(HttpStatusCode.NotFound, new HttpResponseMessage(HttpStatusCode.NotFound)));

            DataElementService sut = SetUpTestService(muescheliClient: muescheliClientMock.Object);

            // Act
            Task Act() => sut.Scan(_defaultDataElementScanRequest);

            // Assert
            MuescheliHttpException exception = await Assert.ThrowsAsync<MuescheliHttpException>(Act);
        }

        [Fact]
        public async Task Scan_RequestFailedException_CallsStorage_LogsWarnings()
        {
            // Arrange
            Mock<IAppOwnerBlob> repository = new();
            Mock<IStorageClient> storageClient = new();
            Mock<ILogger<DataElementService>> loggerMock = new();
            repository
                .Setup(r => r.GetBlobProperties(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()))
                .ThrowsAsync(new RequestFailedException(string.Empty));
            storageClient
                .Setup(s => s.DataElementExists(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(false);

            DataElementService sut = SetUpTestService(
                appOwnerBlob: repository.Object,
                storageClient: storageClient.Object,
                logger: loggerMock.Object);

            // Act
            await sut.Scan(_defaultDataElementScanRequest);

            // Assert
            loggerMock.Verify(x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(2));
            storageClient.Verify(x => x.DataElementExists(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Scan_RequestFailedException_DataElementDoesExist_LogsError_ExceptionBubblesUp()
        {
            // Arrange
            Mock<IAppOwnerBlob> repository = new();
            Mock<IStorageClient> storageClient = new();
            Mock<ILogger<DataElementService>> loggerMock = new();
            repository
                .Setup(r => r.GetBlobProperties(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()))
                .ThrowsAsync(new RequestFailedException(string.Empty));
            storageClient
                .Setup(s => s.DataElementExists(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            DataElementService sut = SetUpTestService(
                appOwnerBlob: repository.Object,
                storageClient: storageClient.Object,
                logger: loggerMock.Object);

            // Act
            Task Act() => sut.Scan(_defaultDataElementScanRequest);

            // Assert
            RequestFailedException exception = await Assert.ThrowsAsync<RequestFailedException>(Act);
            Assert.Equal($"DataElement {_defaultDataElementScanRequest.DataElementId} found and blob does not exist", exception.Message);
            loggerMock.Verify(x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<RequestFailedException>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
            loggerMock.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<RequestFailedException>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
            storageClient.Verify(x => x.DataElementExists(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        private static DataElementService SetUpTestService(
            IAppOwnerBlob appOwnerBlob = null,
            IMuescheliClient muescheliClient = null,
            IStorageClient storageClient = null,
            ILogger<DataElementService> logger = null)
        {
            if (appOwnerBlob is null)
            {
                Mock<IAppOwnerBlob> blobMock = new();
                blobMock
                    .Setup(b => b.GetBlobProperties(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()))
                    .ReturnsAsync(new BlobPropertyModel
                    {
                        LastModified = _matchingTimestamp
                    });
                blobMock
                    .Setup(b => b.GetBlob(It.IsAny<string>(), It.IsAny<string>(), null))
                    .ReturnsAsync((Stream)null);

                appOwnerBlob = blobMock.Object;
            }

            if (muescheliClient is null)
            {
                Mock<IMuescheliClient> muescheliClientMock = new();
                muescheliClientMock.Setup(m => m.ScanStream(It.IsAny<Stream>(), It.IsAny<string>()))
                    .ReturnsAsync(ScanResult.OK);

                muescheliClient = muescheliClientMock.Object;
            }

            if (storageClient is null)
            {
                Mock<IStorageClient> storageClientMock = new();
                storageClientMock
                    .Setup(s => s.PatchFileScanStatus(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<FileScanStatus>()));

                storageClient = storageClientMock.Object;
            }

            if (logger == null)
            {
                logger = new Mock<ILogger<DataElementService>>().Object;
            }

            return new DataElementService(appOwnerBlob, muescheliClient, storageClient, logger);
        }
    }
}
