using RestApi.Common;
using RestApi.DTOS;
using RestApi.Firebase;
using RestApi.HttpClients;
using RestApi.MessageBroker;
using System.Text;

namespace RestApi.Learning
{
    public class LearningManager
    {
        private readonly CacheService _cacheService;
        private readonly ILoggerService _loggerService;
        private readonly EventBus _eventBus;
        private readonly StorageService _firebaseStorageService;

        public AlgorithmName AlgorithmName { get; set; } // this needs to be set based on path
        private readonly int _clientsThresholdToStartTraining = 3;

        public LearningManager(CacheService cacheService, ILoggerService loggerService, EventBus eventBus, StorageService firebaseStorageService)
        {
            _cacheService = cacheService;
            _loggerService = loggerService;
            _eventBus = eventBus;
            _firebaseStorageService = firebaseStorageService;
        }

        public async Task<bool> CheckIfTrainingShouldStartAsync()
        {
            if (!_cacheService.GetStartTraining(AlgorithmName) || _eventBus.aggregationInProgress)
            {
                Console.WriteLine("Server is not prepared to start training!");
                return false;
            }

            var r = await _loggerService.LogAsync("Training is about to start!");
            if (!r)
            {
                Console.WriteLine("Failed to log message");
            }

            return true;
        }

        public async Task<string?> GetModelDownloadUrlAsync()
        {
            if (_eventBus.aggregationInProgress)
            {
                Console.WriteLine("Agregation in progress!");
                return null;
            }

            var fileMetadata = await _loggerService.GetFileMetadata("current_mnist_model");
            if (fileMetadata == null)
            {
                Console.WriteLine("Model not found!");
                return null;
            }

            return await _firebaseStorageService.GetAggregatedModelFileUrl(AlgorithmName.mnist, fileMetadata.firebaseStorageID);
        }

        public async Task<bool> PushFlowAsync(FileContent fileContent)
        {
            if (_eventBus.aggregationInProgress)
            {
                Console.WriteLine("Agregation in progress!");
                return false;
            }

            var clientNumber = await PushClientModelAsync(fileContent);
            if (clientNumber == -1)
            {
                return false;
            }

            if (clientNumber == _clientsThresholdToStartTraining)
            {
                _cacheService.SetStartTraining(AlgorithmName, false);
                _eventBus.aggregationInProgress = true;

                var latestModelFirebaseStorageID = await UpdateClientModelsAsync();
                if (latestModelFirebaseStorageID == null)
                {
                    Console.WriteLine("Failed to update client models");
                    return false;
                }

                _eventBus.PublishAgregateMessage(latestModelFirebaseStorageID);
            }

            return true;
        }

        public async Task<bool> StartTrainingAsync()
        {
            _eventBus.aggregationInProgress = false;
            _cacheService.SetStartTraining(AlgorithmName, true);
            _cacheService.InitializePushedClientsCounter();

            var r = await _firebaseStorageService.CleanupClientModels(AlgorithmName.mnist, "client_mnist_model_");
            if (!r)
            {
                Console.WriteLine("Failed to cleanup client models");
                return false;
            }

            return true;
        }

        private async Task<long> PushClientModelAsync(FileContent fileContent)
        {
            long clientNumber = _cacheService.IncrementPushedClients(AlgorithmName);

            var clientModelName = "client_" + AlgorithmName + "_model_" + clientNumber;
            var fileStreamContent = new MemoryStream(Encoding.UTF8.GetBytes(fileContent.content));

            var r = await _firebaseStorageService.UploadClientModel(fileStreamContent, AlgorithmName.mnist, clientModelName);
            if (!r)
            {
                Console.WriteLine("Failed to upload model");
                return -1;
            }

            Console.WriteLine("Model pushed successfully: " + clientModelName);
            return clientNumber;
        }

        private async Task<string?> UpdateClientModelsAsync()
        {
            var previousModelFileName = "previous_mnist_model";
            var previousModelFileMetadata = await _loggerService.GetFileMetadata(previousModelFileName);
            if (previousModelFileMetadata != null)
            {
                await _firebaseStorageService.DeleteModel(AlgorithmName.mnist, previousModelFileMetadata.firebaseStorageID);
            }
            else
            {
                Console.WriteLine("Previous model not found");
                return null;
            }

            var latestModelFirebaseStorageID = Guid.NewGuid().ToString();
            var r = await _loggerService.SwapModelFiles(latestModelFirebaseStorageID);
            if (!r)
            {
                Console.WriteLine("Failed to swap model files");
                return null;
            }

            return latestModelFirebaseStorageID;
        }
    }
}
