﻿namespace Altinn.FileScan.Models
{
    /// <summary>
    /// This class represents a request to perform a file scan of an Altinn Data Element. 
    /// </summary>
    public class DataElementScanRequest
    {
        /// <summary>
        /// Gets or sets the unique id of the data element.
        /// </summary>
        public string DataElementId { get; set; }

        /// <summary>
        /// Gets or sets the unique id of the parent instance of the data element.
        /// </summary>
        /// <remarks>
        /// The instance id contains both the instance owner party id and the unique instance guid.
        /// </remarks>
        public string InstanceId { get; set; }

        /// <summary>
        /// Gets or sets the name of the data element (file)
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Gets or sets the time when blob was saved.
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the path to blob storage.
        /// </summary>
        public string BlobStoragePath { get; set; }

        /// <summary>
        /// Gets or sets the application owner identifier
        /// </summary>
        public string Org { get; set; }

        /// <summary>
        /// Gets or sets an optional alternate number to append to the storage account name
        /// </summary>
        public int? StorageAccountNumber { get; set; }
    }
}
