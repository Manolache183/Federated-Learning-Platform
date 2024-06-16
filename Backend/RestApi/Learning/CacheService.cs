using RestApi.Common;
using StackExchange.Redis;

namespace RestApi.Learning
{
    public sealed class CacheService
    {
        private const string _connectionString = "redis:6379";
        private const string _pushedClientsPrefix = "pushedClients_";
        private const string _startTrainingPrefix = "startTraining_";
        private const string _lastTrainingPrefix = "lastTrainingTimestamp_";
        private const string _clientsThresholdPrefix = "clientsThresholdToStartTraining_";

        private readonly ConnectionMultiplexer _connectionMultiplexer;
        
        public CacheService()
        {
            _connectionMultiplexer = ConnectionMultiplexer.Connect(_connectionString);
        }

        public long IncrementPushedClients(string clientID)
        {
            var db = _connectionMultiplexer.GetDatabase();
            var key = _pushedClientsPrefix + clientID;

            return db.StringIncrement(key);
        }

        public bool GetStartTraining(string clientID)
        {
            var db = _connectionMultiplexer.GetDatabase();
            var key = _startTrainingPrefix + clientID;

            var value = db.StringGet(key);
            if (value.IsNullOrEmpty)
            {
                SetStartTraining(clientID, false);
                return false;
            }

            return value == "true";
        }

        public void SetStartTraining(string clientID, bool value)
        {
            var db = _connectionMultiplexer.GetDatabase();
            var key = _startTrainingPrefix + clientID;

            if (value)
            {
                db.StringSet(key, "true");
            }
            else
            {
                db.StringSet(key, "false");
            }
        }

        public void InitializePushedClientsCounter(string clientID)
        {
                var db = _connectionMultiplexer.GetDatabase();
                var key = _pushedClientsPrefix + clientID;

                db.StringSet(key, 0);
        }
        
        public void SetLastTrainingTimestamp(string clientID, DateTime timestamp)
        {
            var db = _connectionMultiplexer.GetDatabase();
            var key = _lastTrainingPrefix + clientID;

            db.StringSet(key, timestamp.ToString());
        }

        public DateTime GetLastTrainingTimestamp(string clientID)
        {
            var db = _connectionMultiplexer.GetDatabase();
            var key = _lastTrainingPrefix + clientID;

            var value = db.StringGet(key);
            if (value.IsNullOrEmpty)
            {
                return DateTime.MinValue;
            }

            return DateTime.Parse(value);
        }

        public void SetClientsThresholdToStartTraining(string clientID, int threshold)
        {
            var db = _connectionMultiplexer.GetDatabase();
            var key = _clientsThresholdPrefix + clientID;

            db.StringSet(key, threshold);
        }

        public int GetClientsThresholdToStartTraining(string clientID)
        {
            var db = _connectionMultiplexer.GetDatabase();
            var key = _clientsThresholdPrefix + clientID;

            var value = db.StringGet(key);
            if (value.IsNullOrEmpty)
            {
                return -1;
            }

            return (int)value;
        }
    }
}
