using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestApi.Firebase;
using RestApi.DTOS;
using RestApi.HttpClients;
using RestApi.MessageBroker;
using System.Net;

namespace RestApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MnistController : ControllerBase
    {
        private readonly ILoggerService _loggerService;
        private readonly EventBus _eventBus;
        private readonly StorageService _firebaseStorageService;
        private int pushedClients = 0;
        private readonly int clientsThresholdToStartTraining = 3;
        private bool startTraining = false;

        public MnistController(ILoggerService loggerService, EventBus eventBus, StorageService firebaseStorageService)
        {
            _loggerService = loggerService;
            _eventBus = eventBus;
            _firebaseStorageService = firebaseStorageService;
        }

        [HttpGet("checkIfTrainingShouldStart")]
        public async Task<IActionResult> CheckIfTrainingShouldStart()
        {
            if (!startTraining || _eventBus.aggregationInProgress)
            {
                return await Task.FromResult(StatusCode((int)HttpStatusCode.ServiceUnavailable, "Server is not prepared to start training!"));
            }

            return await Task.FromResult(Ok("Training should start!"));
        }

        [HttpGet("pullModel")]
        public async Task<IActionResult> PullModel()
        {
            if (_eventBus.aggregationInProgress)
            {
                return await Task.FromResult(StatusCode((int)HttpStatusCode.ServiceUnavailable, "Agregation in progress!"));
            }


            return await Task.FromResult(Ok("Here is the current model!"));
        }

        [HttpPost("pushModel")]
        public async Task<IActionResult> PushModel()
        {
            if (_eventBus.aggregationInProgress)
            {
                return await Task.FromResult(StatusCode((int)HttpStatusCode.ServiceUnavailable, "Agregation already started!"));
            }

            pushedClients++;

            if (pushedClients == clientsThresholdToStartTraining)
            {
                _eventBus.PublishAgregateMessage();
                startTraining = false;
                _eventBus.aggregationInProgress = true;
            }

            return await Task.FromResult(Ok("Model pushed!"));
        }

        [HttpPost("initializeTrainig")]
        public async Task<IActionResult> StartTraining()
        {
            _eventBus.aggregationInProgress = false;
            pushedClients = 0;
            startTraining = true;

            return await Task.FromResult(Ok("Training can be started!"));
        }
    }
}
