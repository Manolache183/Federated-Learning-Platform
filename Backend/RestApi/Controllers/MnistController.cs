using Microsoft.AspNetCore.Mvc;
using RestApi.Firebase;
using RestApi.HttpClients;
using RestApi.MessageBroker;
using System.Net;
using RestApi.Common;
using System.Text;
using RestApi.DTOS;
using Microsoft.AspNetCore.Authorization;

namespace RestApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MnistController : ControllerBase
    {
        private readonly ILoggerService _loggerService;
        private readonly EventBus _eventBus;
        private readonly StorageService _firebaseStorageService;
        
        private readonly int clientsThresholdToStartTraining = 3;
        
        private static bool startTraining = false;
        private static int pushedClients = 0;
        private static readonly object pushedClientsLock = new object();

        public MnistController(ILoggerService loggerService, EventBus eventBus, StorageService firebaseStorageService)
        {
            _loggerService = loggerService;
            _eventBus = eventBus;
            _firebaseStorageService = firebaseStorageService;
        }

        [Authorize(Roles = "client")]
        [HttpGet("checkIfTrainingShouldStart")]
        public async Task<IActionResult> CheckIfTrainingShouldStart()
        {
            if (!startTraining || _eventBus.aggregationInProgress)
            {
                return StatusCode((int)HttpStatusCode.ServiceUnavailable, "Server is not prepared to start training!");
            }

            var r = await _loggerService.LogAsync("Training should start!");
            if (!r)
            {
                Console.WriteLine("Failed to log message");
            }

            return Ok("Training should start!");
        }

        [Authorize(Roles = "client")]
        [HttpGet("getModelDownloadUrl")]
        public async Task<IActionResult> GetModelDownloadUrl()
        {
            if (_eventBus.aggregationInProgress)
            {
                return StatusCode((int)HttpStatusCode.ServiceUnavailable, "Agregation in progress!");
            }

            var fileMetadata = await _loggerService.GetFileMetadata("current_mnist_model");
            if (fileMetadata == null)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "Model not found!");
            }

            var downloadUrl = await _firebaseStorageService.GetAggregatedModelFileUrl(AlgorithmName.mnist, fileMetadata.firebaseStorageID);
            if (downloadUrl == null)
            {
                startTraining = false;
                return StatusCode((int)HttpStatusCode.InternalServerError, "Failed to get download url!");
            }

            return Ok(downloadUrl);
        }

        [Authorize(Roles = "client")]
        [HttpPost("pushModel")]
        public async Task<IActionResult> PushModel([FromBody] FileContent fileContent)
        {
            // test
            if (_eventBus.aggregationInProgress)
            {
                return StatusCode((int)HttpStatusCode.ServiceUnavailable, "Agregation already started!");
            }

            int clientNumber;
            lock (pushedClientsLock)
            {
                pushedClients++;
                clientNumber = pushedClients;
            }

            const string clientModelNamePrefix = "client_mnist_model_";
            var clientModelName = clientModelNamePrefix + pushedClients;

            var fileStreamContent = new MemoryStream(Encoding.UTF8.GetBytes(fileContent.content));
            
            var r = await _firebaseStorageService.UploadClientModel(fileStreamContent, AlgorithmName.mnist, clientModelName);
            if (!r)
            {
                Console.WriteLine("Failed to upload model");
                return StatusCode((int)HttpStatusCode.InternalServerError, "Failed to upload model!");
            }

            Console.WriteLine("Model pushed successfully: " + clientModelName);

            if (clientNumber == clientsThresholdToStartTraining)
            {
                startTraining = false;
                _eventBus.aggregationInProgress = true;

                var latestModelFirebaseStorageID = Guid.NewGuid().ToString();

                var previousModelFileName = "previous_mnist_model";
                var previousModelFileMetadata = await _loggerService.GetFileMetadata(previousModelFileName);
                if (previousModelFileMetadata != null)
                {
                    await _firebaseStorageService.DeleteModel(AlgorithmName.mnist, previousModelFileMetadata.firebaseStorageID);
                } else
                {
                    Console.WriteLine("Previous model not found");
                }
                
                r = await _loggerService.SwapModelFiles(latestModelFirebaseStorageID);
                if (!r)
                {
                    Console.WriteLine("Failed to swap model files");
                }

                _eventBus.PublishAgregateMessage(latestModelFirebaseStorageID);
            }

            return Ok("Model pushed!");
        }

        [HttpPost("initializeTrainig")]
        public async Task<IActionResult> StartTraining()
        {
            _eventBus.aggregationInProgress = false;
            pushedClients = 0;
            startTraining = true;

            var r = await _firebaseStorageService.CleanupClientModels(AlgorithmName.mnist, "client_mnist_model_");
            if (!r)
            {
                Console.WriteLine("Failed to cleanup client models");
            }

            return Ok("Training can be started!");
        }
    }
}
