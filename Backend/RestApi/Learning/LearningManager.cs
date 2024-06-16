using Newtonsoft.Json;
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
        private readonly IClientPlatformService _clientPlatformService;

        private const int _defaultClientsThresholdToStartTraining = 3;

        public LearningManager(CacheService cacheService, ILoggerService loggerService, EventBus eventBus, StorageService firebaseStorageService, IClientPlatformService clientPlatformService)
        {
            _cacheService = cacheService;
            _loggerService = loggerService;
            _eventBus = eventBus;
            _firebaseStorageService = firebaseStorageService;
            _clientPlatformService = clientPlatformService;
        }

        public async Task<bool> CheckIfTrainingShouldStartAsync(string clientID)
        {
            if (_eventBus.aggregationInProgress)
            {
                Console.WriteLine("Server is not prepared to start training!");
                return false;
            }

            if (!_cacheService.GetStartTraining(clientID))
            {
                var trainingInterval = await _clientPlatformService.GetClientTrainingInterval(clientID);
                var lastTrainingTimestamp = _cacheService.GetLastTrainingTimestamp(clientID);

                if (DateTime.Now - lastTrainingTimestamp < TimeSpan.FromMinutes(trainingInterval))
                {
                    Console.WriteLine("Client is not ready to start training!");
                    return false;
                }

                _cacheService.SetLastTrainingTimestamp(clientID, DateTime.Now);
                _cacheService.SetStartTraining(clientID, true);
            }
            
            var r = await _loggerService.LogAsync("Training is about to start!");
            if (!r)
            {
                Console.WriteLine("Failed to log message");
            }

            return true;
        }

        public async Task<string?> GetModelDownloadUrlAsync(string clientID)
        {
            if (_eventBus.aggregationInProgress)
            {
                Console.WriteLine("Agregation in progress!");
                return null;
            }

            var currModel = clientID + "_current_model";
            var fileMetadata = await _loggerService.GetFileMetadata(currModel);
            if (fileMetadata == null)
            {
                Console.WriteLine("Model not found!");
                return null;
            }

            return await _firebaseStorageService.GetAggregatedModelFileUrl(fileMetadata.firebaseStorageID);
        }

        public async Task<bool> PushFlowAsync(List<ModelParameter> modelParameters, string clientID)
        {
            if (_eventBus.aggregationInProgress)
            {
                Console.WriteLine("Agregation in progress!");
                return false;
            }

            var clientNumber = await PushClientModelAsync(modelParameters, clientID);
            if (clientNumber == -1)
            {
                return false;
            }

            var clientsThreshold = _cacheService.GetClientsThresholdToStartTraining(clientID);
            if (clientsThreshold == -1)
            {
                clientsThreshold = await InitializeClientsThreshold(clientID);
                _cacheService.SetClientsThresholdToStartTraining(clientID, clientsThreshold);
            }

            if (clientNumber == clientsThreshold)
            {
                _cacheService.SetStartTraining(clientID, false);
                _eventBus.aggregationInProgress = true;

                var latestModelFirebaseStorageID = await UpdateClientModelsAsync(clientID);
                if (latestModelFirebaseStorageID == null)
                {
                    Console.WriteLine("Failed to update client models");
                    return false;
                }

                _eventBus.PublishAgregateMessage(latestModelFirebaseStorageID);
            }

            return true;
        }

        public async Task<bool> StartTrainingAsync(string clientID)
        {
            _eventBus.aggregationInProgress = false;
            _cacheService.SetStartTraining(clientID, true);
            _cacheService.InitializePushedClientsCounter(clientID);

            var r = await _firebaseStorageService.CleanupClientModels(clientID + "_client_model_");
            if (!r)
            {
                Console.WriteLine("Failed to cleanup client models");
                return false;
            }

            return true;
        }

        public async Task<bool> InitializeFileMetadata(string clientID)
        {
            var previousModelFileName = clientID + "_previous_model";
            var currentModelFileName = clientID + "_current_model";

            var previousModelFileMetadata = await _loggerService.AddFileMetadata(previousModelFileName, "NA");
            var currentModelFileMetadata = await _loggerService.AddFileMetadata(currentModelFileName, "NA");

            if (!previousModelFileMetadata || !currentModelFileMetadata)
            {
                Console.WriteLine("Failed to add file metadata");
                return false;
            }

            return true;
        }

        private async Task<long> PushClientModelAsync(List<ModelParameter> modelParameters, string clientID)
        {
            long clientNumber = _cacheService.IncrementPushedClients(clientID);

            var clientModelName = clientID + "_client_model_" + clientNumber;

            var model = ParseParameters(modelParameters);
            var fileStreamContent = new MemoryStream(Encoding.UTF8.GetBytes(model));

            var r = await _firebaseStorageService.UploadClientModel(fileStreamContent, clientModelName);
            if (!r)
            {
                Console.WriteLine("Failed to upload model");
                return -1;
            }

            Console.WriteLine("Model pushed successfully: " + clientModelName);
            return clientNumber;
        }

        private string ParseParameters(List<ModelParameter> modelParameters)
        {
            var allParameters = new Dictionary<string, string>();
            foreach (var param in modelParameters)
            {
                byte[] layerBytes = Convert.FromBase64String(param.value);
                Console.WriteLine("Layer: " + param.name + " Data: " + layerBytes.Length + " bytes");
                allParameters[param.name] = param.value;
            }

            return JsonConvert.SerializeObject(allParameters);
        }

        private async Task<string?> UpdateClientModelsAsync(string clientID)
        {
            var previousModelFileName = clientID + "_previous_model";
            var previousModelFileMetadata = await _loggerService.GetFileMetadata(previousModelFileName);

            if(previousModelFileMetadata == null)
            {
                Console.WriteLine("Previous model not found");
                return null;
            }

            if (previousModelFileMetadata.firebaseStorageID != "NA")
            {
                await _firebaseStorageService.DeleteModel(previousModelFileMetadata.firebaseStorageID);
            }
            else
            {
                Console.WriteLine("Previous file metadata points to nothing, first or second training round");
            }

            var latestModelFirebaseStorageID = clientID + "_" + Guid.NewGuid().ToString();

            var r = await _loggerService.SwapModelFiles(latestModelFirebaseStorageID);
            if (!r)
            {
                Console.WriteLine("Failed to swap model files");
                return null;
            }

            return latestModelFirebaseStorageID;
        }

        private async Task<int> InitializeClientsThreshold(string clientID)
        {
            var r = await _clientPlatformService.GetClientThreshold(clientID);
            if (r == -1)
            {
                Console.WriteLine("Failed to get client threshold, fallback to default value: " + _defaultClientsThresholdToStartTraining);
                return _defaultClientsThresholdToStartTraining;
            }

            return r;
        }
    }
}
