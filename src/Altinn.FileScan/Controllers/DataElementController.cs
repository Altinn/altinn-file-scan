﻿using Altinn.FileScan.Services.Interfaces;
using Altinn.Platform.Storage.Interface.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.FileScan.Controllers
{
    /// <summary>
    /// Controller containing all actions related to data element
    /// </summary>
    [Route("filescan/api/v1/dataelement")]
    [ApiController]
    public class DataElementController : ControllerBase
    {
        private readonly IDataElement _dataElement;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataElementController"/> class.
        /// </summary>
        /// <param name="dataElement">The data element service</param>
        public DataElementController(IDataElement dataElement)
        {
            _dataElement = dataElement;
        }

        /// <summary>
        /// Post a data element for malware scan
        /// </summary>
        /// [Authorize(Policy = "PlatformAccess")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        public async Task<ActionResult> Scan(DataElement dataElement)
        {
            await _dataElement.Scan(dataElement);
            return Ok();
        }
    }
}