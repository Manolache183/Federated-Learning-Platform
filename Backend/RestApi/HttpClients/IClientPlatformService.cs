using RestApi.Common;

namespace RestApi.HttpClients
{
    public interface IClientPlatformService
    {
        public abstract Task NotifyClient(string clientID, TrainingInfo trainingInfo);
        public abstract Task<int> GetClientTrainingInterval(string clientID);
        public abstract Task<int> GetClientThreshold(string clientID);
    }
}
