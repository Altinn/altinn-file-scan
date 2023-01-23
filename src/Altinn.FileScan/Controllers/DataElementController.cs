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
        /// <summary>
        /// Post a data element for malware scan
        /// </summary>
        [Authorize(Policy = "PlatformAccess")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        public ActionResult ScanDataElement(DataElement input)
        {
            return Ok();
        }
    }
}
