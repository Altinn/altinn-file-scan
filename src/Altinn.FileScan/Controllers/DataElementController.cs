using Altinn.FileScan.Models;
using Altinn.FileScan.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.FileScan.Controllers;

/// <summary>
/// Controller containing all actions related to data element
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DataElementController"/> class.
/// </remarks>
[Route("filescan/api/v1/dataelement")]
[ApiController]
public class DataElementController(IDataElement dataElement) : ControllerBase
{
    /// <summary>
    /// Post a data element for malware scan
    /// </summary>
    [Authorize(Policy = "PlatformAccess")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpPost]
    public async Task<ActionResult> Scan(DataElementScanRequest scanRequest)
    {
        await dataElement.Scan(scanRequest);

        return Ok();
    }
}
