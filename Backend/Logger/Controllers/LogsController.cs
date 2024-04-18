using Microsoft.AspNetCore.Mvc;
using Logger.CloudDatabase;

namespace Logger.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LogsController : ControllerBase
    {
        private readonly DatabaseService _databaseService;

        public LogsController(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }
        
        [HttpGet]
        public async Task<IActionResult> Ping()
        {
            Console.WriteLine("Ping request received");
            return await Task.Run(Ok);
        }

        [HttpPost("logLearningCoordonator")]
        public async Task<IActionResult> LogLearningCoordonator([FromBody] LogItemDto payload)
        {
            Console.WriteLine("Received log request from microservice: {0}", payload.MicroserviceName);
            var r = await _databaseService.AddLogItem(new LogItem(Guid.NewGuid(), payload.MicroserviceName, DateTime.Now));
            if (r)
            {
                return Ok("Logged item");
            }

            return StatusCode(500, "Failed to log item");
        }
    }
}
