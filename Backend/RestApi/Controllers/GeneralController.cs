using Microsoft.AspNetCore.Mvc;
using RestApi.CloudDatabase;
using RestApi.HttpClients;

namespace RestApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GeneralController : ControllerBase
    {
        private readonly ILoggerService _loggerService;
        private readonly DatabaseService _databaseService;

        public GeneralController(ILoggerService loggerService, DatabaseService databaseService)
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
            var r = await _loggerService.LogAsync();
            Console.WriteLine(r);

            if (r.IsSuccessStatusCode)
            {
                return Ok();
            }

            return StatusCode(500, "nu merse log-ul");
        }

        [HttpGet("testDownload")]
        public async Task<IActionResult> TestDownload()
        {
            await _databaseService.PrintFileContents("text_test_file.txt");
            return Ok();
        }
    }
}
