namespace Altinn.FileScan.Services.Interfaces
{
    /// <summary>
    /// Interface containing all actions for an Altinn app owner Azure Key Vault
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
