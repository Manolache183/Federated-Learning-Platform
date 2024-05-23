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

        public long IncrementPushedClients(AlgorithmName algorithm)
        {
            var db = _connectionMultiplexer.GetDatabase();
            var key = _pushedClientsPrefix + algorithm;

            return db.StringIncrement(key);
        }

        public bool GetStartTraining(AlgorithmName algorithm)
        {
            var db = _connectionMultiplexer.GetDatabase();
            var key = _startTrainingPrefix + algorithm;

            return db.StringGet(key) == "true";
        }

        public void SetStartTraining(AlgorithmName algorithm, bool value)
        {
            var db = _connectionMultiplexer.GetDatabase();
            var key = _startTrainingPrefix + algorithm;

            if (value)
            {
                db.StringSet(key, "true");
            }
            else
            {
                db.StringSet(key, "false");
            }
        }

        public void InitializeStartTraining()
        {
            foreach (AlgorithmName algorithm in Enum.GetValues(typeof(AlgorithmName)))
            {
                var db = _connectionMultiplexer.GetDatabase();
                var key = _startTrainingPrefix + algorithm;

                db.StringSet(key, "false");
            }
        }

        public void InitializePushedClientsCounter()
        {
            foreach (AlgorithmName algorithm in Enum.GetValues(typeof(AlgorithmName)))
            {
                var db = _connectionMultiplexer.GetDatabase();
                var key = _pushedClientsPrefix + algorithm;

                db.StringSet(key, 0);
            }
        }
    }
}
