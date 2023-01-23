using Altinn.FileScan.Services.Interfaces;
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
        private readonly ILogger<DataElementController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataElementController"/> class.
        /// </summary>
        public DataElementController(IDataElement dataElement, ILogger<DataElementController> logger)
        {
            _dataElement = dataElement;
            _logger = logger;
        }

        /// <summary>
        /// Post a data element for malware scan
        /// </summary>
        [Authorize(Policy = "PlatformAccess")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        public async Task<ActionResult> Scan(DataElement dataElement)
        {
            _logger.LogInformation(" // DataElementController // Scan // Calling service to scan dataElement");
            bool successful = await _dataElement.Scan(dataElement);

            if (!successful)
            { 
                return StatusCode(500);
            }

            return Ok();
        }
    }
}
