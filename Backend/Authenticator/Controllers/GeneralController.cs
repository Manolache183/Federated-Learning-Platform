using Microsoft.AspNetCore.Mvc;

namespace Authenticator.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GeneralController : ControllerBase
    {
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("Pong");
        }
    }
}
