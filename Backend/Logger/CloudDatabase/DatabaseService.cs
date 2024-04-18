using Google.Cloud.Firestore;

namespace Logger.CloudDatabase
{
    public class DatabaseService
    {
        private readonly string _path = AppDomain.CurrentDomain.BaseDirectory + @"federated-learning-platf-c15c0-firebase-adminsdk-slcw0-90585331c4.json";
        private const string _projectID = "federated-learning-platf-c15c0";
        private const string _collectionName = "logs";
        private readonly FirestoreDb _db;

        public DatabaseService()
        {
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", _path);
            _db = FirestoreDb.Create(_projectID);
        }

        public async Task<bool> AddLogItem(LogItem logItem)
        {
            DocumentReference documentReference = _db.Collection(_collectionName).Document(logItem.Id.ToString());
            Dictionary<string, object> logItemDictionary = new Dictionary<string, object>
            {
                { "MicroserviceName", logItem.MicroserviceName },
                { "Timestamp", logItem.Timestamp.ToUniversalTime() }
            };

            try
            {
                await documentReference.SetAsync(logItemDictionary);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }
    }
}
