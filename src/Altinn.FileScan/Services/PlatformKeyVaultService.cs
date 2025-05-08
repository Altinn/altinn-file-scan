using System.Security.Cryptography.X509Certificates;
using Altinn.Common.AccessToken.Configuration;
using Altinn.FileScan.Services.Interfaces;

using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;

using Microsoft.Extensions.Options;

namespace Altinn.FileScan.Services
{
    /// <summary>
    /// Implementation of  <see cref="IPlatformKeyVault"/> using default azure credentials to access the key vault defined in <see cref="KeyVaultSettings"/>
    /// </summary>
    public class PlatformKeyVaultService : IPlatformKeyVault
    {
        private readonly string _vaultUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlatformKeyVaultService"/> class.
        /// </summary>
        public PlatformKeyVaultService(IOptions<KeyVaultSettings> keyVaultSettings)
        {
            _vaultUri = keyVaultSettings.Value.SecretUri;
        }

        /// <inheritdoc/>
        public async Task<X509Certificate2> GetCertificateAsync(string certId)
        {
            CertificateClient certificateClient = new(new Uri(_vaultUri), new DefaultAzureCredential());
            AsyncPageable<CertificateProperties> certificatePropertiesPage = certificateClient.GetPropertiesOfCertificateVersionsAsync(certId);
            await foreach (CertificateProperties certificateProperties in certificatePropertiesPage)
            {
                if (certificateProperties.Enabled == true &&
                    (certificateProperties.ExpiresOn == null || certificateProperties.ExpiresOn >= DateTime.UtcNow))
                {
                    X509Certificate2 certificate = await certificateClient.DownloadCertificateAsync(certificateProperties.Name, certificateProperties.Version);
                    return certificate;
                }
            }

            return null;
        }
    }
}
