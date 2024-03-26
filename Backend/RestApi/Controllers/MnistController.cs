using Microsoft.AspNetCore.Mvc;
using RestApi.HttpClients;
using System.Net;

namespace RestApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MnistController : ControllerBase // eventually turn this into a partial class?
    {
        private readonly IAggregatorService _aggregatorService;
        private readonly ILoggerService _loggerService;

        public MnistController(ILoggerService loggerService, IAggregatorService aggregatorService)
        {
            _loggerService = loggerService;
            _aggregatorService = aggregatorService;
        }

        [HttpGet("pull_mnist_model")]
        public async Task<IActionResult> PullLearningModelAsync()
        {
            // eventually will need to check that the user is in the mnist group
            // logs everywhere
            return await Task.Run(Ok); 
            var response = await _aggregatorService.PullLearningModelAsync();
            if (response.IsSuccessStatusCode) // add more response codes
            {
                var contentStream = await response.Content.ReadAsStreamAsync(); // should the content creation be here?
                var contentType = response?.Content?.Headers?.ContentType?.MediaType;
                var fileName = "learning_model";

                if (contentType != null) { 
                    return File(contentStream, contentType, fileName);
                }
            }
            
            return BadRequest("Server couldn't retrieve the model");
        }

        [HttpPost("send_mnist_weights")]
        public async Task<IActionResult> SendWeightsAsync([FromForm] IFormFile file)
        {
            // check that the user is in the mnist group
            return await Task.Run(Ok);
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                var response = await _aggregatorService.SendWeightsAsync(memoryStream, file.FileName);
                if (response.StatusCode == HttpStatusCode.OK) // add more response codes
                {
                    return Ok();
                }
            }
            
            return BadRequest();
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

            return BadRequest();
        }

        [HttpGet("ping")]
        public async Task<IActionResult> Ping()
        {
            return await Task.Run(Ok);
        }
    }
}
