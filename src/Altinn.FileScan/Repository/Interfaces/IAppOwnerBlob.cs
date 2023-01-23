namespace Altinn.FileScan.Repository.Interfaces
{
    /// <summary>
    /// Interface for interacting with a blob repository
    /// </summary>
    public interface IAppOwnerBlob
    {
        /// <summary>
        /// Retrieves a data blob as a stream from an organisation storage account within Altinn
        /// </summary>
        /// <param name="org">The short name of the organisation</param>
        /// <param name="blobPath">Full path to the blob within a storage account</param>
        /// <returns></returns>
        public Task<Stream> GetBlob(string org, string blobPath);
    }
}
