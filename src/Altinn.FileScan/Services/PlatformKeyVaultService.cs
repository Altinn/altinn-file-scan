#nullable disable

using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Altinn.Common.AccessToken.Configuration;
using Altinn.FileScan.Services.Interfaces;
using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Microsoft.Extensions.Options;

namespace Altinn.FileScan.Services;

/// <summary>
/// Implementation of  <see cref="IPlatformKeyVault"/> using default azure credentials to access the key vault defined in <see cref="KeyVaultSettings"/>
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PlatformKeyVaultService"/> class.
/// </remarks>
public class PlatformKeyVaultService(IOptions<KeyVaultSettings> keyVaultSettings) : IPlatformKeyVault
{
    private readonly string _vaultUri = keyVaultSettings.Value.SecretUri;

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
