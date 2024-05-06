using Microsoft.AspNetCore.Mvc;
using RestApi.Firebase;
using RestApi.HttpClients;

namespace RestApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GeneralController : ControllerBase
    {
        private readonly ILoggerService _loggerService;
        private readonly StorageService _databaseService;

        public GeneralController(ILoggerService loggerService, StorageService databaseService)
        {
            _loggerService = loggerService;
            _databaseService = databaseService;
        }

        [HttpGet("ping")]
        public async Task<IActionResult> Ping()
        {
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
