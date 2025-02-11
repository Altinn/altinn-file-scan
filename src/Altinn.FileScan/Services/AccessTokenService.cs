using System.Security.Cryptography.X509Certificates;

using Altinn.Common.AccessTokenClient.Configuration;
using Altinn.Common.AccessTokenClient.Services;
using Altinn.FileScan.Services.Interfaces;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Altinn.FileScan.Services
{
    /// <summary>
    /// Implementation of the <see cref="IAccessToken"/> using key vault to retrieve sertificates and generating token
    /// </summary>
    public class AccessTokenService : IAccessToken
    {
        private readonly IPlatformKeyVault _keyVault;
        private readonly IAccessTokenGenerator _accessTokenGenerator;
        private readonly IMemoryCache _cache;
        private readonly MemoryCacheEntryOptions _cacheOptions;

        private const string CertId = "platform-access-token-private-cert";

        /// <summary>
        /// Initializes a new instance of the <see cref="AccessTokenService"/> class.
        /// </summary>
        public AccessTokenService(IPlatformKeyVault keyVault, IAccessTokenGenerator accessTokenGenerator, IMemoryCache cache, IOptions<AccessTokenSettings> settings)
        {
            _keyVault = keyVault;
            _accessTokenGenerator = accessTokenGenerator;
            _cache = cache;

            _cacheOptions = new()
            {
                AbsoluteExpiration = new DateTimeOffset(DateTime.Now.AddSeconds(settings.Value.TokenLifetimeInSeconds - 2))
            };
        }

        /// <summary>
        /// Generates an access token with issuer `platform` and app `file-scan`
        /// </summary>
        public async Task<string> Generate()
        {
            var accessTokenCacheKey = "accesstoken-platform-file-scan";

            if (_cache.TryGetValue(accessTokenCacheKey, out string accessToken))
            {
                return accessToken;
            }

            string certBase64 =
                  await _keyVault.GetCertificateAsync(CertId);

            var certificate = X509CertificateLoader.LoadPkcs12(Convert.FromBase64String(certBase64), (string)null, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
            accessToken = _accessTokenGenerator.GenerateAccessToken("platform", "file-scan", certificate);

            _cache.Set(accessTokenCacheKey, accessToken, _cacheOptions);

            return accessToken;
        }
    }
}
