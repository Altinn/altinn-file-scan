#nullable disable

using System.Security.Cryptography.X509Certificates;
using Altinn.Common.AccessTokenClient.Configuration;
using Altinn.Common.AccessTokenClient.Services;
using Altinn.FileScan.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Altinn.FileScan.Services;

/// <summary>
/// Implementation of the <see cref="IAccessToken"/> using key vault to retrieve sertificates and generating token
/// </summary>
public class AccessTokenService(IPlatformKeyVault keyVault, IAccessTokenGenerator accessTokenGenerator, IMemoryCache cache, IOptions<AccessTokenSettings> settings) : IAccessToken
{
    private readonly MemoryCacheEntryOptions _cacheOptions = new()
    {
        AbsoluteExpiration = new DateTimeOffset(DateTime.Now.AddSeconds(settings.Value.TokenLifetimeInSeconds - 2))
    };

    private const string CertId = "platform-access-token-private-cert";

    /// <summary>
    /// Generates an access token with issuer `platform` and app `file-scan`
    /// </summary>
    public async Task<string> Generate()
    {
        var accessTokenCacheKey = "accesstoken-platform-file-scan";

        if (cache.TryGetValue(accessTokenCacheKey, out string accessToken))
        {
            return accessToken;
        }

        X509Certificate2 certificate = await keyVault.GetCertificateAsync(CertId);

        accessToken = accessTokenGenerator.GenerateAccessToken("platform", "file-scan", certificate);

        cache.Set(accessTokenCacheKey, accessToken, _cacheOptions);

        return accessToken;
    }
}
