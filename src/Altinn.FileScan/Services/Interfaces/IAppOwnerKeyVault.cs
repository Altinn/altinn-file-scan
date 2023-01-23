namespace Altinn.FileScan.Services.Interfaces
{
    /// <summary>
    /// Describes any implementation of a KeyVault service towards an app owner key vault
    /// </summary>
    public interface IAppOwnerKeyVault
    {
        /// <summary>
        /// Gets the value of a secret from the given key vault.
        /// </summary>
        /// <param name="vaultUri">The URI of the key vault to ask for secret.</param>
        /// <param name="secretId">The id of the secret.</param>
        /// <returns>The secret value.</returns>
        Task<string> GetSecretAsync(string vaultUri, string secretId);
    }
}
