using System.Threading.Tasks;

using Altinn.FileScan.Configuration;
using Altinn.FileScan.Repository;
using Altinn.FileScan.Services.Interfaces;

using Microsoft.Extensions.Options;

using Moq;

using Xunit;

namespace Altinn.FileScan.Tests.TestingRepositories
{
    public class BlobContainerClientProviderTests
    {
        private AppOwnerAzureStorageConfig _storageConfig;

        public BlobContainerClientProviderTests()
        {
            _storageConfig = new AppOwnerAzureStorageConfig
            {
                OrgKeyVaultURI = "https =//{0}-dev-keyvault.vault.azure.net/",
                OrgStorageAccount = "{0}altinndevstrg01",
                OrgStorageContainer = "{0}-dev-appsdata-blob-db",
                OrgSasDefinition = "{0}devsasdef01",
                AllowedSasTokenAgeHours = 8
            };
        }

        [Fact]
        public async Task GetBlobContainerClient_TokenInCache_StillYoungTrue_ReturnedFromCache()
        {
            // Arrange 
            Mock<IAppOwnerKeyVault> keyVaultMock = new();
            keyVaultMock.Setup(kv => kv.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("sasToken");

            var sut = new BlobContainerClientProvider(
                keyVaultMock.Object,
                Options.Create(_storageConfig),
                null);

            await sut.GetBlobContainerClient("ttd", null);

            // Act
            var actual = await sut.GetBlobContainerClient("ttd", null);

            // Assert
            Assert.NotNull(actual);
            keyVaultMock.Verify(kv => kv.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>()), Times.AtMostOnce);
        }

        [Fact]
        public async Task GetBlobContainerClient_TokenInCache_StillYoungFalse_NewClientCreatedAndCached()
        {
            // Arrange 
            Mock<IAppOwnerKeyVault> keyVaultMock = new();
            keyVaultMock.Setup(kv => kv.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("sasToken");
            _storageConfig.AllowedSasTokenAgeHours = 0;

            var sut = new BlobContainerClientProvider(
                            keyVaultMock.Object,
                            Options.Create(_storageConfig),
                            null);

            await sut.GetBlobContainerClient("ttd", null);

            // Act
            var actual = await sut.GetBlobContainerClient("ttd", null);

            // Assert
            Assert.NotNull(actual);
            keyVaultMock.Verify(kv => kv.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
        }

        [Theory]
        [InlineData("ttd", "https://ttd-sa.blob.core.windows.net/ttd-sc?sasToken=value")]
        [InlineData("skd", "https://skd-sa.blob.core.windows.net/skd-sc?sasToken=value")]
        [InlineData("nav", "https://nav-sa.blob.core.windows.net/nav-sc?sasToken=value")]
        public async Task GetBlobUri_StringFormatationSuccesful(string org, string expectedUri)
        {
            // Arrange       
            Mock<IAppOwnerKeyVault> keyVaultMock = new();
            keyVaultMock.Setup(kv => kv.GetSecretAsync(
                It.Is<string>(s => s.Equals($"https://{org}.kv.com")),
                It.Is<string>(s => s.Equals($"{org}-sa-{org}-sasdef"))))
                .ReturnsAsync("sasToken=value");

            var sut = new BlobContainerClientProvider(
                    keyVaultMock.Object,
                    Options.Create(new AppOwnerAzureStorageConfig
                    {
                        OrgKeyVaultURI = "https://{0}.kv.com",
                        OrgStorageAccount = "{0}-sa",
                        OrgSasDefinition = "{0}-sasdef",
                        OrgStorageContainer = "{0}-sc"
                    }),
                    null);

            // Act
            var actual = await sut.GetBlobUri(org, null);

            // Assert
            keyVaultMock.VerifyAll();
            Assert.Equal(expectedUri, actual.ToString());
        }

        [Fact]
        public async Task GetBlobUri_OrgKvUsesAlternativeName_NameRetrievedFromDictionary()
        {
            // Arrange
            Mock<IAppOwnerKeyVault> keyVaultMock = new();
            keyVaultMock.Setup(kv => kv.GetSecretAsync(
                It.Is<string>(s => s.Equals("https://random-uri.com")),
                It.Is<string>(s => s.Equals("ttd-sa-ttd-sasdef"))))
                .ReturnsAsync("sasToken=value");

            var sut = new BlobContainerClientProvider(
                    keyVaultMock.Object,
                    Options.Create(new AppOwnerAzureStorageConfig
                    {
                        OrgKeyVaultURI = "https://{0}.kv.com",
                        OrgKeyVaultDict = "{\"ttd\":\"https://random-uri.com\"}",
                        OrgStorageAccount = "{0}-sa",
                        OrgSasDefinition = "{0}-sasdef",
                        OrgStorageContainer = "{0}-sc"
                    }),
                    null);

            // Act
            var actualDefaultContainer = await sut.GetBlobUri("ttd", null);
            var actualAlternateContainer = await sut.GetBlobUri("ttd", 2);

            // Assert
            keyVaultMock.VerifyAll();
            Assert.Equal("/ttd-sc-2", actualAlternateContainer.AbsolutePath);
            Assert.Equal("/ttd-sc", actualDefaultContainer.AbsolutePath);
        }
    }
}
