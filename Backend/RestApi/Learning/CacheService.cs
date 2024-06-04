using RestApi.Common;
using StackExchange.Redis;

namespace RestApi.Learning
{
    public sealed class CacheService
    {
        private const string _connectionString = "redis-service:6379";
        private const string _pushedClientsPrefix = "pushedClients_";
        private const string _startTrainingPrefix = "startTraining_";

        private readonly ConnectionMultiplexer _connectionMultiplexer;
        
        public CacheService()
        {
            _connectionMultiplexer = ConnectionMultiplexer.Connect(_connectionString);
        }

        public long IncrementPushedClients(AlgorithmName algorithm, string clientID)
        {
            var db = _connectionMultiplexer.GetDatabase();
            var key = _pushedClientsPrefix + clientID + "_" + algorithm;

            return db.StringIncrement(key);
        }

        public bool GetStartTraining(AlgorithmName algorithm, string clientID)
        {
            var db = _connectionMultiplexer.GetDatabase();
            var key = _startTrainingPrefix + clientID + "_" +  algorithm;

            var value = db.StringGet(key);
            if (value.IsNullOrEmpty)
            {
                SetStartTraining(algorithm, clientID, false);
                return false;
            }

            return value == "true";
        }

        public void SetStartTraining(AlgorithmName algorithm, string clientID, bool value)
        {
            var db = _connectionMultiplexer.GetDatabase();
            var key = _startTrainingPrefix + clientID + "_" +  algorithm;

            if (value)
            {
                db.StringSet(key, "true");
            }
            else
            {
                db.StringSet(key, "false");
            }
        }

        public void InitializePushedClientsCounter(AlgorithmName algorithm, string clientID)
        {
                var db = _connectionMultiplexer.GetDatabase();
                var key = _pushedClientsPrefix + clientID + "_" + algorithm;

                db.StringSet(key, 0);
        }
    }
}
