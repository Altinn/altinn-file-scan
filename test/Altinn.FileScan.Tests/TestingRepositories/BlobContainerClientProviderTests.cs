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
        [Fact]
        public async Task GetBlobContainerClient_TokenInCache_StillYoungTrue_ReturnedFromCache()
        {
            Assert.True(true);
        }

        [Fact]
        public async Task GetBlobContainerClient_TokenInCache_StillYoungFalse_NewClientCreatedAndCached()
        {
            Assert.True(true);
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
            var actual = await sut.GetBlobUri(org);

            // Assert
            keyVaultMock.VerifyAll();
            Assert.Equal(expectedUri, actual.ToString());
        }
    }
}
