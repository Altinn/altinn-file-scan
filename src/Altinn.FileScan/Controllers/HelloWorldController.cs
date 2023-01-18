using Microsoft.AspNetCore.Mvc;

namespace FileScan.Controllers
{
    /// <summary>
    /// Collection of endpoint related to hello worlds
    /// </summary>
    [Route("api/hello")]
    [ApiController]
    public class HelloWorldController : ControllerBase
    {
        /// <summary>
        /// Simple hello world endpoint for testing
        /// </summary>
        /// <returns>A string</returns>
        [HttpGet]
        public ActionResult<string> Hello()
        {
            return Ok("hello world");
        }
    }
}
