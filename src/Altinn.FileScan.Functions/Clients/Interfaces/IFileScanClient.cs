using System.Threading.Tasks;
using Altinn.Platform.Storage.Interface.Models;

namespace Altinn.FileScan.Functions.Clients.Interfaces
{
    /// <summary>
    /// Interface to FileScan API
    /// </summary>
    public interface IFileScanClient
    {
        /// <summary>
        /// Send dataElement for file scanning.
        /// </summary>
        /// <param name="dataElement">DataElement to send</param>
        Task PostFileScan(string dataElement);
    }
}
