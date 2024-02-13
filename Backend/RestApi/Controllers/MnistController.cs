using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using RestApi.HttpClients;
using System.Net;
using RestApi.Hubs;

namespace RestApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MnistController : ControllerBase // eventually turn this into a partial class?
    {
        private readonly ILogger<MnistController> _logger;
        private readonly IAggregatorService _aggregatorService;
        private readonly IHubContext<ClientHub, IClientHub> _hubContext;

        public MnistController(ILogger<MnistController> logger, IAggregatorService aggregatorService, IHubContext<ClientHub, IClientHub> hubContext)
        {
            _logger = logger;
            _aggregatorService = aggregatorService;
            _hubContext = hubContext;
        }

        [HttpGet("pull_mnist_model")]
        public async Task<IActionResult> PullLearningModelAsync()
        {
            // eventually will need to check that the user is in the mnist group

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

        [HttpPost("upload_mnist_aggregated_model")]
        public async Task<IActionResult> ReceiveMnistMode() // [FromForm] IFormFile file
        {
            await _hubContext.Clients.Group("mnist").ReceiveNotification("New model aggregated");
            return Ok();
            //using (var memoryStream = new MemoryStream())
            //{
            //    await file.CopyToAsync(memoryStream);
            //    return Ok();
            //}
        }

        [HttpGet("test_kotlin")]
        public async Task<IActionResult> TestKotlin()
        {
            Console.WriteLine("Test Kotlin");
            return Ok();
        }
    }
}
