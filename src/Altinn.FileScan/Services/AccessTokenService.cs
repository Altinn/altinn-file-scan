using System.Security.Cryptography.X509Certificates;

using Altinn.Common.AccessToken.Configuration;
using Altinn.Common.AccessTokenClient.Services;
using Altinn.FileScan.Configuration;
using Altinn.FileScan.Services.Interfaces;

using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Secrets;

namespace Altinn.FileScan.Services
{
    /// <summary>
    /// Implementation of the <see cref="IAccessToken"/> using key vault to retrieve sertificates and generating token
    /// </summary>
    public class AccessTokenService : IAccessToken
    {
        private readonly IPlatformKeyVault _keyVault;
        private readonly IAccessTokenGenerator _accessTokenGenerator;
        private const string CertId = "platform-access-token-private-cert";

        /// <summary>
        /// Initializes a new instance of the <see cref="AccessTokenService"/> class.
        /// </summary>
        public AccessTokenService(IPlatformKeyVault keyVault, IAccessTokenGenerator accessTokenGenerator)
        {
            _keyVault = keyVault;
            _accessTokenGenerator = accessTokenGenerator;
        }

        /// <inheritdoc/>
        public async Task<string> Generate(string issuer = "platform", string app = "file-scan")
        {
            string certBase64 =
                       await _keyVault.GetCertificateAsync(CertId);

            string accessToken = _accessTokenGenerator.GenerateAccessToken("platform", app, new X509Certificate2(
                Convert.FromBase64String(certBase64),
                (string)null,
                X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable));
            return accessToken;
        }
    }
}
