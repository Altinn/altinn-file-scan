#nullable disable

using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Altinn.FileScan.Functions.Configuration;
using Altinn.FileScan.Functions.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Altinn.FileScan.Functions.Services;

/// <summary>
/// Class to resolve certificate to generate an access token
/// </summary>
/// <remarks>
/// Default constructor
/// </remarks>
/// <param name="logger">The logger</param>
/// <param name="certificateResolverSettings">Settings for certificate resolver</param>
/// <param name="keyVaultService">Key vault service</param>
/// <param name="keyVaultSettings">Key vault settings</param>
public class CertificateResolverService(
    ILogger<CertificateResolverService> logger,
    IOptions<CertificateResolverSettings> certificateResolverSettings,
    IKeyVaultService keyVaultService,
    IOptions<KeyVaultSettings> keyVaultSettings) : ICertificateResolverService
{
    private readonly CertificateResolverSettings _certificateResolverSettings = certificateResolverSettings.Value;
    private readonly KeyVaultSettings _keyVaultSettings = keyVaultSettings.Value;
    private DateTime _reloadTime = DateTime.MinValue;
    private X509Certificate2 _cachedX509Certificate = null;
    private readonly object _lockObject = new object();

    /// <summary>
    /// Find the configured
    /// </summary>
    /// <returns></returns>
    public async Task<X509Certificate2> GetCertificateAsync()
    {
        if (DateTime.UtcNow > _reloadTime || _cachedX509Certificate == null)
        {
            var certificate = await keyVaultService.GetCertificateAsync(
                _keyVaultSettings.KeyVaultURI,
                _keyVaultSettings.PlatformCertSecretId);
            lock (_lockObject)
            {
                _cachedX509Certificate = certificate;

                _reloadTime = DateTime.UtcNow.AddSeconds(_certificateResolverSettings.CacheCertLifetimeInSeconds);
                logger.LogInformation("Certificate reloaded.");
            }
        }

        return _cachedX509Certificate;
    }
}
