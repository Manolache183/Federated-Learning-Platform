using Microsoft.AspNetCore.Mvc;
using System.Net;
using RestApi.Common;
using RestApi.DTOS;
using Microsoft.AspNetCore.Authorization;
using RestApi.Learning;

namespace RestApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MnistController : ControllerBase
    {
        private const AlgorithmName _algorithmName = AlgorithmName.mnist;
        private readonly LearningManager _learningManager;

        public MnistController(LearningManager learningManager)
        {
            _learningManager = learningManager;
            _learningManager.AlgorithmName = _algorithmName;
        }

        // [Authorize(Roles = "client")]
        [HttpGet("checkIfTrainingShouldStart/{clientID}")]
        public async Task<IActionResult> CheckIfTrainingShouldStart(string clientID)
        {
            var startTraining = await _learningManager.CheckIfTrainingShouldStartAsync(clientID);
            if (!startTraining)
            {
                return StatusCode((int)HttpStatusCode.ServiceUnavailable, "Server is not prepared to start training!");
            }

            return Ok("Training should start!");
        }

        // [Authorize(Roles = "client")]
        [HttpGet("getModelDownloadUrl/{clientID}")]
        public async Task<IActionResult> GetModelDownloadUrl(string clientID)
        {
            var downloadUrl = await _learningManager.GetModelDownloadUrlAsync(clientID);
            if (downloadUrl == null)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "No model has been aggregated yet for this client!");
            }

            return Ok(downloadUrl);
        }

        // [Authorize(Roles = "client")]
        [HttpPost("pushModel/{clientID}")]
        public async Task<IActionResult> PushModel([FromBody] List<ModelParameter> modelParameters, string clientID)
        {
            var r = await _learningManager.PushFlowAsync(modelParameters, clientID);
            if (!r)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "Server problem");
            }

            return Ok("Model pushed!");
        }

        [HttpPost("initializeTrainig/{clientID}")]
        public async Task<IActionResult> StartTraining(string clientID)
        {
            var r = await _learningManager.StartTrainingAsync(clientID);
            if (!r)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "Server problem");
            }

            return Ok("Training can be started!");
        }

        [HttpPost("initializeFileMetadata/{clientID}")]
        public async Task<IActionResult> InitializeFileMetadata(string clientID)
        {
            var r = await _learningManager.InitializeFileMetadata(clientID);
            if (!r)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "Server problem");
            }

            return Ok("File metadata initialized!");
        }
          
    }
}
