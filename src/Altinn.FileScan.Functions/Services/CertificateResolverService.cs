using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

using Altinn.FileScan.Functions.Configuration;
using Altinn.FileScan.Functions.Services.Interfaces;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Altinn.FileScan.Functions.Services
{
    /// <summary>
    /// Class to resolve certificate to generate an access token
    /// </summary>
    public class CertificateResolverService : ICertificateResolverService
    {
        private readonly ILogger<CertificateResolverService> _logger;
        private readonly CertificateResolverSettings _certificateResolverSettings;
        private readonly IKeyVaultService _keyVaultService;
        private readonly KeyVaultSettings _keyVaultSettings;
        private DateTime _reloadTime;
        private X509Certificate2 _cachedX509Certificate = null;
        private readonly object _lockObject = new object();

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="certificateResolverSettings">Settings for certificate resolver</param>
        /// <param name="keyVaultService">Key vault service</param>
        /// <param name="keyVaultSettings">Key vault settings</param>
        public CertificateResolverService(
            ILogger<CertificateResolverService> logger,
            IOptions<CertificateResolverSettings> certificateResolverSettings,
            IKeyVaultService keyVaultService,
            IOptions<KeyVaultSettings> keyVaultSettings)
        {
            _logger = logger;
            _certificateResolverSettings = certificateResolverSettings.Value;
            _keyVaultService = keyVaultService;
            _keyVaultSettings = keyVaultSettings.Value;
            _reloadTime = DateTime.MinValue;
        }

        /// <summary>
        /// Find the configured 
        /// </summary>
        /// <returns></returns>
        public async Task<X509Certificate2> GetCertificateAsync()
        {
            if (DateTime.UtcNow > _reloadTime || _cachedX509Certificate == null)
            {
                string certBase64 = await _keyVaultService.GetCertificateAsync(
                    _keyVaultSettings.KeyVaultURI,
                    _keyVaultSettings.PlatformCertSecretId);

                lock (_lockObject)
                {
                    _cachedX509Certificate = X509CertificateLoader.LoadPkcs12(Convert.FromBase64String(certBase64), null, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);

                    _reloadTime = DateTime.UtcNow.AddSeconds(_certificateResolverSettings.CacheCertLifetimeInSeconds);
                    _logger.LogInformation("Certificate reloaded.");
                }
            }

            return _cachedX509Certificate;
        }
    }
}
