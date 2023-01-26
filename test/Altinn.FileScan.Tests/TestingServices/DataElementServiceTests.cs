using System.IO;
using System.Threading.Tasks;

using Altinn.FileScan.Clients.Interfaces;
using Altinn.FileScan.Models;
using Altinn.FileScan.Repository.Interfaces;
using Altinn.FileScan.Services;
using Altinn.FileScan.Services.Interfaces;
using Altinn.Platform.Storage.Interface.Enums;
using Altinn.Platform.Storage.Interface.Models;

using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

namespace Altinn.FileScan.Tests.TestingServices
{
    public class DataElementServiceTests
    {
        [Fact]
        public async Task Scan_HappyPath_AllServicesAreCalledWithExpectedData()
        {
            // Arraange
            Mock<IAppOwnerBlob> blobMock = new();
            blobMock
                .Setup(b => b.GetBlob(It.Is<string>(s => s == "ttd"), It.Is<string>(s => s == "blobstoragePath/org/attachment.pdf")))
                .ReturnsAsync((Stream)null);

            Mock<IMuescheliClient> muescheliClientMock = new();
            muescheliClientMock.Setup(m => m.ScanStream(It.IsAny<Stream>(), It.Is<string>(s => s == "attachment.pdf")))
                .ReturnsAsync(ScanResult.OK);

            Mock<IStorageClient> storageClientMock = new();
            storageClientMock
                .Setup(s => s.PatchFileScanStatus(It.Is<string>(s => s == "instanceId"), It.Is<string>(s => s == "dataElementId"), It.Is<FileScanStatus>(f => f.FileScanResult == FileScanResult.Clean)));

            DataElementService sut = SetUpTestService(blobMock.Object, muescheliClientMock.Object, storageClientMock.Object);

            var input = new DataElementScanRequest
            {
                BlobStoragePath = "blobstoragePath/org/attachment.pdf",
                DataElementId = "dataElementId",
                InstanceId = "instanceId",
                Filename = "attachment.pdf",
                Org = "ttd"
            };

            // Act
            await sut.Scan(input);

            // Assert
            blobMock.VerifyAll();
            muescheliClientMock.VerifyAll();
            storageClientMock.VerifyAll();
        }

        [Fact]
        public async Task Scan_FilenameMissing_DataElementIdIsUsed()
        {
            // Arraange
            Mock<IMuescheliClient> muescheliClientMock = new();
            muescheliClientMock.Setup(m => m.ScanStream(It.IsAny<Stream>(), It.Is<string>(s => s == "dataElementId.txt")))
                .ReturnsAsync(ScanResult.OK);

            DataElementService sut = SetUpTestService(muescheliClient: muescheliClientMock.Object);

            var input = new DataElementScanRequest
            {
                BlobStoragePath = "blobstoragePath/org/attachment.pdf",
                DataElementId = "dataElementId",
                InstanceId = "instanceId",
                Org = "ttd"
            };

            // Act
            await sut.Scan(input);

            // Assert
            muescheliClientMock.VerifyAll();
        }

        [Theory]
        [InlineData("OK", "Clean")]
        public void Scan_DeterminateScanResult_MappedCorrectly(string scanResult, string expectedFileScanResult)
        {
            // inline data with ok and infected results. 
        }

        private static DataElementService SetUpTestService(IAppOwnerBlob appOwnerBlob = null, IMuescheliClient muescheliClient = null, IStorageClient storageClient = null)
        {
            if (appOwnerBlob is null)
            {
                var blobMock = new Mock<IAppOwnerBlob>();

                blobMock
                    .Setup(b => b.GetBlob(It.IsAny<string>(), It.IsAny<string>()))
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

            var logger = new Mock<ILogger<IDataElement>>();

            return new DataElementService(appOwnerBlob, muescheliClient, storageClient, logger.Object);
        }
    }
}
