using Google.Cloud.Firestore;

namespace Logger.Firebase
{
    public class FirestoreDatabaseService
    {
        private readonly string _path = AppDomain.CurrentDomain.BaseDirectory + @"federated-learning-platform-secrets.json";
        private const string _projectID = "federated-learning-platf-c15c0";
        private readonly FirestoreDb _db;

        private enum CollectionNames
        {
            Logs,
            FilesMetadata
        }

        public FirestoreDatabaseService()
        {
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", _path);
            _db = FirestoreDb.Create(_projectID);
        }

        public async Task<bool> AddLogItem(LogItem logItem)
        {
            DocumentReference documentReference = _db.Collection(CollectionNames.Logs.ToString()).Document(logItem.id.ToString());
            Dictionary<string, object> logItemDictionary = new Dictionary<string, object>
            {
                { "microserviceName", logItem.microserviceName },
                { "message", logItem.message },
                { "timestamp", logItem.timestamp.ToUniversalTime() }
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

        public async Task<bool> AddFileMetadata(FileMetadata fileMetadata)
        {
            DocumentReference documentReference = _db.Collection(CollectionNames.FilesMetadata.ToString()).Document(fileMetadata.fileName);
            Dictionary<string, object> fileMetadataDictionary = new Dictionary<string, object>
            {
                { "firebaseStorageID", fileMetadata.firebaseStorageID.ToString() },
                { "leastAccessed", fileMetadata.leastAccessed.ToUniversalTime() }
            };

            try
            {
                await documentReference.SetAsync(fileMetadataDictionary);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        public async Task<FileMetadataSend?> GetFileMetadata(string fileName)
        {
            DocumentReference documentReference = _db.Collection(CollectionNames.FilesMetadata.ToString()).Document(fileName);
            DocumentSnapshot documentSnapshot = await documentReference.GetSnapshotAsync();

            var dict = documentSnapshot.ToDictionary();

            var firebaseStorageID = dict["firebaseStorageID"].ToString();
            var leastAccessed = dict["leastAccessed"].ToString();

            if (firebaseStorageID == null || leastAccessed == null)
            {
                Console.WriteLine("file metadata not found");
                return null;
            }

            Console.WriteLine("File metadata: " + fileName + " " + firebaseStorageID + " " + leastAccessed);

            return new FileMetadataSend(firebaseStorageID, leastAccessed);
        }

        public async Task<bool> UpdateFileMetadata(FileMetadata fileMetadata)
        {
            DocumentReference documentReference = _db.Collection(CollectionNames.FilesMetadata.ToString()).Document(fileMetadata.fileName);
            Dictionary<string, object> fileMetadataDictionary = new Dictionary<string, object>
            {
                { "firebaseStorageID", fileMetadata.firebaseStorageID.ToString() },
                { "leastAccessed", fileMetadata.leastAccessed.ToUniversalTime() }
            };

            try
            {
                await documentReference.SetAsync(fileMetadataDictionary);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        public async Task<bool> DeleteFileMetadata(string fileName)
        {
            DocumentReference documentReference = _db.Collection(CollectionNames.FilesMetadata.ToString()).Document(fileName);

            try
            {
                await documentReference.DeleteAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        public async Task<bool> SwapModelFiles(string latestModelFirebaseStorageID)
        {
            var clientID = latestModelFirebaseStorageID.Split('_')[0];
            
            var currentModelFileMetadata = await GetFileMetadata(clientID + "_current_model");
            if (currentModelFileMetadata == null)
            {
                Console.WriteLine("Current model not found");
                return false;
            }

            var previousModelFileMetadataUpdate = new FileMetadata(clientID + "_previous_model", currentModelFileMetadata.firebaseStorageID, DateTime.Now);
            var currentModelFileMetadataUpdate = new FileMetadata(clientID + "_current_model", latestModelFirebaseStorageID, DateTime.Now);

            var r = await UpdateFileMetadata(previousModelFileMetadataUpdate);
            if (!r)
            {
                Console.WriteLine("Failed to update previous model metadata");
                return false;
            }

            r = await UpdateFileMetadata(currentModelFileMetadataUpdate);
            if (!r)
            {
                Console.WriteLine("Failed to update current model metadata");
                return false;
            }

            return true;
        }
    }
}
