using Microsoft.AspNetCore.Mvc;
using RestApi.HttpClients;

namespace RestApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GeneralController : ControllerBase
    {
        private readonly ILoggerService _loggerService;

        public GeneralController(ILoggerService loggerService)
        {
            _loggerService = loggerService;
        }

        [HttpGet("ping")]
        public async Task<IActionResult> Ping()
        {
            Console.WriteLine("Pinged");
            return await Task.Run(Ok);
        }

        [HttpPost("log")]
        public async Task<IActionResult> Log()
        {
            Console.WriteLine("Logging");
            var r = await _loggerService.LogAsync("Sample log");

            if (r)
            {
                return Ok();
            }

            return StatusCode(500, "nu merse log-ul");
        }
    }
}
