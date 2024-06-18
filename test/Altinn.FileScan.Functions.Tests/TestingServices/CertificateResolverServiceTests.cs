using Altinn.FileScan.Functions.Configuration;
using Altinn.FileScan.Functions.Services;
using Altinn.FileScan.Functions.Services.Interfaces;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using Xunit;

namespace Altinn.FileScan.Tests
{
    public class CertificateResolverServiceTests
    {
        private readonly Mock<ILogger<CertificateResolverService>> _mockLogger = new Mock<ILogger<CertificateResolverService>>();
        private readonly IOptions<CertificateResolverSettings> _certificateResolverSettings = Options.Create(new CertificateResolverSettings { CacheCertLifetimeInSeconds = 1 });
        private readonly Mock<IKeyVaultService> _mockKeyVaultService = new Mock<IKeyVaultService>();
        private readonly IOptions<KeyVaultSettings> _keyVaultSettings = Options.Create(new KeyVaultSettings());

        [Fact]
        public async Task GetCertificateAsync_ReturnsCachedCertificate_WhenCalledMultipleTimesWithinCacheLifetime()
        {
            // Arrange
            var certBase64 = File.ReadAllLines(@$"base64-encoded-cert.txt")[0];
            _mockKeyVaultService.Setup(s => s.GetCertificateAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(certBase64)
                .Verifiable();

            var resolverService = new CertificateResolverService(_mockLogger.Object, _certificateResolverSettings, _mockKeyVaultService.Object, _keyVaultSettings);

            // Act
            await resolverService.GetCertificateAsync();
            await resolverService.GetCertificateAsync();

            // Assert
            _mockKeyVaultService.Verify(s => s.GetCertificateAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetCertificateAsync_ReloadsCertificate_WhenCalledAfterCacheLifetime()
        {
            // Arrange
            var certBase64 = File.ReadAllLines(@$"base64-encoded-cert.txt")[0];
            _mockKeyVaultService.Setup(s => s.GetCertificateAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(certBase64)
                .Verifiable();

            var resolverService = new CertificateResolverService(_mockLogger.Object, _certificateResolverSettings, _mockKeyVaultService.Object, _keyVaultSettings);

            // Act
            await resolverService.GetCertificateAsync();
            await Task.Delay(2000); // Wait for longer than the cache lifetime
            await resolverService.GetCertificateAsync();

            // Assert
            _mockKeyVaultService.Verify(s => s.GetCertificateAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
        }
    }
}
