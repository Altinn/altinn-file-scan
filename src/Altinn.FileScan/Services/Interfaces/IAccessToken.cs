namespace Altinn.FileScan.Services.Interfaces
{
    /// <summary>
    /// Interface for the access token service
    /// </summary>
    public interface IAccessToken
    {
        /// <summary>
        /// Generates an access token based on the provided issuer and app
        /// </summary>
        /// <returns>An access token</returns>
        public Task<string> Generate(string issuer = "platform", string app = "file-scan");
    }
}
