﻿namespace Altinn.FileScan.Services.Interfaces
{
    /// <summary>
    /// Interface containing all actions for an Altinn Platform Azure Key Vault
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
