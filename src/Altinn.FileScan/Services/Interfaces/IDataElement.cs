﻿using Altinn.FileScan.Models;

namespace Altinn.FileScan.Services.Interfaces
{
    /// <summary>
    /// Interface for all operations related to file scan of a a data element
    /// </summary>
    public interface IDataElement
    {
        /// <summary>
        /// Initiates the process to scan the provided data element for malware
        /// </summary>
        /// <returns>Returns true if the scan was completes successfully or not, regardles of scan result</returns>
        public Task Scan(DataElementScanRequest scanRequest);
    }
}
