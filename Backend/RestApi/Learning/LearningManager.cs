using RestApi.Common;
using RestApi.DTOS;
using RestApi.Firebase;
using RestApi.HttpClients;
using RestApi.MessageBroker;
using System.Net;
using System.Text;

namespace RestApi.Learning
{
    public class LearningManager
    {
        private readonly ILoggerService _loggerService;
        private readonly EventBus _eventBus;
        private readonly StorageService _firebaseStorageService;

        private readonly string _algorithmName;
        private readonly int _clientsThresholdToStartTraining;

        private static bool _startTraining = false; // from redis
        private static int _pushedClients = 0; // from redis
        private static object? _pushedClientsLock;

        public LearningManager(ILoggerService loggerService, EventBus eventBus, StorageService firebaseStorageService, object pushedClientsLock, int clientsThresholdToStartTraining = 3, string algorithmName = "mnist")
        {
            _loggerService = loggerService;
            _eventBus = eventBus;
            _firebaseStorageService = firebaseStorageService;

            _clientsThresholdToStartTraining = clientsThresholdToStartTraining;
            _algorithmName = algorithmName;

            _pushedClientsLock = pushedClientsLock;
        }

        public async Task<bool> CheckIfTrainingShouldStart()
        {
            if (!_startTraining || _eventBus.aggregationInProgress)
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

        public async Task<bool> GetModelDownloadUrl()
        {
            if (_eventBus.aggregationInProgress)
            {
                Console.WriteLine("Agregation in progress!"); // aici cam e Service unavailable
                return false;
            }

            var fileMetadata = await _loggerService.GetFileMetadata("current_mnist_model");
            if (fileMetadata == null)
            {
                Console.WriteLine("Model not found!");
                return false;
            }

            var downloadUrl = await _firebaseStorageService.GetAggregatedModelFileUrl(AlgorithmName.mnist, fileMetadata.firebaseStorageID);
            if (downloadUrl == null)
            {
                _startTraining = false;
                Console.WriteLine("Failed to get download url!");
                return false;
            }

            return true;
        }

        public async Task<bool> PushFlow(FileContent fileContent)
        {
            if (_eventBus.aggregationInProgress)
            {
                Console.WriteLine("Agregation in progress!");
                return false;
            }

            var clientNumber = await PushClientModel(fileContent);
            if (clientNumber == -1)
            {
                return false;
            }

            if (clientNumber == _clientsThresholdToStartTraining)
            {
                _startTraining = false;
                _eventBus.aggregationInProgress = true;

                var latestModelFirebaseStorageID = await UpdateClientModels();
                if (latestModelFirebaseStorageID == null)
                {
                    Console.WriteLine("Failed to update client models");
                    return false;
                }

                _eventBus.PublishAgregateMessage(latestModelFirebaseStorageID);
            }

            return true;
        }

        public async Task<bool> StartTraining()
        {
            _eventBus.aggregationInProgress = false;
            _pushedClients = 0;
            _startTraining = true;

            var r = await _firebaseStorageService.CleanupClientModels(AlgorithmName.mnist, "client_mnist_model_");
            if (!r)
            {
                Console.WriteLine("Failed to cleanup client models");
                return false;
            }

            return true;
        }

        private async Task<int> PushClientModel(FileContent fileContent)
        {
            int clientNumber;
            lock (_pushedClientsLock)
            {
                _pushedClients++;
                clientNumber = _pushedClients;
            }

            var clientModelName = "client_" + _algorithmName + "_model_" + _pushedClients;
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

        private async Task<string?> UpdateClientModels()
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
