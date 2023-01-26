namespace Altinn.FileScan.Services.Interfaces
{
    /// <summary>
    /// Interface for the access token service
    /// </summary>
    public interface IAccessToken
    {
        /// <summary>
        /// Generates an access token
        /// </summary>
        public Task<string> Generate();
    }
}
