namespace Altinn.FileScan.Services.Interfaces
{
    /// <summary>
    /// Describes any implementation of a KeyVault service towards the Altinn Platform key vault
    /// </summary>
    public interface IPlatformKeyVault
    {
        /// <summary>
        /// Gets the certificate from the given key vault.
        /// </summary>
        /// <param name="certId">The id of the secret.</param>
        /// <returns>The certificate value.</returns>
        Task<string> GetCertificateAsync(string certId);
    }
}
