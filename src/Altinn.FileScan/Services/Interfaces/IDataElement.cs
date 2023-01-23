using Altinn.Platform.Storage.Interface.Models;

namespace Altinn.FileScan.Services.Interfaces
{
    /// <summary>
    /// Interface for all operations related to a data element
    /// </summary>
    public interface IDataElement
    {
        /// <summary>
        /// Initiates the process to scan the provided data element for malware
        /// </summary>
        /// <returns>Returns if the scan was completes successfully or not, regardles of scan result</returns>
        public Task<bool> Scan(DataElement dataElement);
    }
}
