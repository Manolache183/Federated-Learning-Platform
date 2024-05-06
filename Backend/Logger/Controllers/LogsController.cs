using Microsoft.AspNetCore.Mvc;
using Logger.Firebase;

namespace Logger.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LogsController : ControllerBase
    {
        private readonly FirestoreDatabaseService _cloudFirestoreService;

        public LogsController(FirestoreDatabaseService cloudFirestoreService)
        {
            _cloudFirestoreService = cloudFirestoreService;
        }
        
        [HttpGet]
        public async Task<IActionResult> Ping()
        {
            Console.WriteLine("Ping request received");
            return await Task.Run(Ok);
        }

        [HttpPost("logLearningCoordonator")]
        public async Task<IActionResult> LogLearningCoordonator([FromBody] LogItemReceive payload)
        {
            Console.WriteLine("Received log request from microservice: {0}", payload.microserviceName);
            var r = await _cloudFirestoreService.AddLogItem(new LogItem(Guid.NewGuid(), payload.microserviceName, payload.message, DateTime.Now));
            if (r)
            {
                return Ok("Logged item");
            }

            return StatusCode(500, "Failed to log item");
        }
    }
}
