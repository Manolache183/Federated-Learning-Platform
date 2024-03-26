using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Logger.DataModels;
using Logger.DTOS;

namespace Logger.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LogsController : ControllerBase
    {
        private readonly string _databaseName = "LogsDatabase";
        private readonly string _containerId = "Logs";

        private readonly CosmosClient _cosmosClient;
        private readonly Database _database;
        private readonly Container _container;

        public LogsController(CosmosClient cosmosClient)
        {
            _cosmosClient = cosmosClient;
            _database = _cosmosClient.GetDatabase(_databaseName);
            _container = _database.GetContainer(_containerId);
        }

        [HttpGet]
        public async Task<IActionResult> Ping()
        {
            return await Task.Run(Ok);
        }

        [HttpPost("logLearningCoordonator")]
        public async Task<IActionResult> LogLearningCoordonator([FromBody] LogItemDTO payload)
        {
            Console.WriteLine("Received log request");
            var logItem = new LogItem(Guid.NewGuid(), payload.microserviceName, payload.timestamp);
            await _container.CreateItemAsync(logItem);
            return Ok();
        }
    }
}
