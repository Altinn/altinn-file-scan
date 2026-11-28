#nullable disable

using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Altinn.Common.AccessTokenClient.Services;
using Altinn.FileScan.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Altinn.FileScan.Services;

/// <summary>
/// Implementation of the <see cref="IAccessToken"/> using key vault to retrieve sertificates and generating token
/// </summary>
public class AccessTokenService(
    IPlatformKeyVault keyVault,
    IAccessTokenGenerator accessTokenGenerator,
    IMemoryCache cache
) : IAccessToken
{
    private const string CertId = "platform-access-token-private-cert";
    private const string CertificateCacheKey = "platform-access-token-private-cert";

    // The generator caches each token for nearly its lifetime, so only the certificate is cached here;
    // caching the token as well could hand out a token the generator created close to its expiry.
    private static readonly TimeSpan _certificateCacheLifetime = TimeSpan.FromHours(1);

    /// <summary>
    /// Generates an access token with issuer `platform` and app `file-scan`
    /// </summary>
    public async Task<string> Generate()
    {
        if (!cache.TryGetValue(CertificateCacheKey, out X509Certificate2 certificate))
        {
            certificate = await keyVault.GetCertificateAsync(CertId);
            if (certificate is not null)
            {
                cache.Set(CertificateCacheKey, certificate, _certificateCacheLifetime);
            }
        }

        return accessTokenGenerator.GenerateAccessToken("platform", "file-scan", certificate);
    }
}
