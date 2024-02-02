using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace RestApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LearningController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<LearningController> _logger;

        public LearningController(ILogger<LearningController> logger)
        {
            _logger = logger;
        }

        [HttpGet("client-0")]
        public async Task<IActionResult> SendPingToWorker()
        {
            var url = "http://client-0:5000/ping";

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                var responseString = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseString);
                if (responseString.Contains("\"message\":\"OK\""))
                {
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
        }

        [HttpPost("trigger_learning")]
        public async Task<IActionResult> TriggerLearning()
        {
            int numClients;
            try
            {
                var numClientsStr = Environment.GetEnvironmentVariable("NUM_CLIENTS");
                if (numClientsStr == null)
                {
                    return BadRequest();
                }
                numClients = int.Parse(numClientsStr);
            }
            catch (Exception)
            {
                return BadRequest(); // change this
            }

            string? url;
            for (int i = 0; i < numClients; i++)
            {
                url = $"http://client-{i}:5000/train_and_send";
                using (var client = new HttpClient())
                {
                    var response = await client.PostAsync(url, new StringContent(""));
                    Console.WriteLine(response.StatusCode);
                    if (response.StatusCode != HttpStatusCode.Accepted)
                    {
                        return BadRequest();
                    }
                }
            }

            return Ok();
        }
    }
}
