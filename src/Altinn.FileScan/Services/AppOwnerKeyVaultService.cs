using Altinn.FileScan.Services.Interfaces;

using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace Altinn.FileScan.Services
{
    /// <summary>
    /// Implementation of <see cref="IAppOwnerKeyVault"/> default credentials are utilized for access
    /// </summary>
    public class AppOwnerKeyVaultService : IAppOwnerKeyVault
    {      
        /// <inheritdoc/>
        public async Task<string> GetSecretAsync(string vaultUri, string secretId)
        {
            SecretClient secretClient = new(new Uri(vaultUri), new DefaultAzureCredential());

            KeyVaultSecret secret = await secretClient.GetSecretAsync(secretId);

            return secret.Value;
        }
    }
}
