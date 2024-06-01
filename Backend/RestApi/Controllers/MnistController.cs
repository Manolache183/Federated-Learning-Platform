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

        [Authorize(Roles = "client")]
        [HttpGet("checkIfTrainingShouldStart")]
        public async Task<IActionResult> CheckIfTrainingShouldStart()
        {
            var startTraining = await _learningManager.CheckIfTrainingShouldStartAsync();
            if (!startTraining)
            {
                return StatusCode((int)HttpStatusCode.ServiceUnavailable, "Server is not prepared to start training!");
            }

            return Ok("Training should start!");
        }

        [Authorize(Roles = "client")]
        [HttpGet("getModelDownloadUrl")]
        public async Task<IActionResult> GetModelDownloadUrl()
        {
            var downloadUrl = await _learningManager.GetModelDownloadUrlAsync();
            if (downloadUrl == null)
            {
                return StatusCode((int)HttpStatusCode.ServiceUnavailable, "Agregation in progress!");
            }

            return Ok(downloadUrl);
        }

        [Authorize(Roles = "client")]
        [HttpPost("pushModel")]
        public async Task<IActionResult> PushModel([FromBody] List<ModelParameter> modelParameters)
        {
            var r = await _learningManager.PushFlowAsync(modelParameters);
            if (!r)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "Server problem");
            }

            return Ok("Model pushed!");
        }

        [HttpPost("initializeTrainig")]
        public async Task<IActionResult> StartTraining()
        {
            var r = await _learningManager.StartTrainingAsync();
            if (!r)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "Server problem");
            }

            return Ok("Training can be started!");
        }
    }
}
