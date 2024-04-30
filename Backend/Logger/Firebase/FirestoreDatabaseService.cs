using Google.Cloud.Firestore;

namespace Logger.Firebase
{
    public class FirestoreDatabaseService
    {
        private readonly string _path = AppDomain.CurrentDomain.BaseDirectory + @"federated-learning-platf-c15c0-firebase-adminsdk-slcw0-90585331c4.json";
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
            DocumentReference documentReference = _db.Collection(CollectionNames.Logs.ToString()).Document(logItem.Id.ToString());
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

        public async Task<bool> AddFileMetadata(FileMetadata fileMetadata)
        {
            DocumentReference documentReference = _db.Collection(CollectionNames.FilesMetadata.ToString()).Document(fileMetadata.FileName);
            Dictionary<string, object> fileMetadataDictionary = new Dictionary<string, object>
            {
                { "FirebaseStorageID", fileMetadata.FirebaseStorageID.ToString() },
                { "LeastAccesed", fileMetadata.LeastAccesed.ToUniversalTime() }
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

            var firebaseStorageID = dict["FirebaseStorageID"].ToString();
            var leastAccesed = dict["LeastAccesed"].ToString();

            if (firebaseStorageID == null || leastAccesed == null)
            {
                Console.WriteLine("File metadata not found");
                return null;
            }

            Console.WriteLine("File metadata: " + fileName + " " + firebaseStorageID + " " + leastAccesed);

            return new FileMetadataSend(firebaseStorageID, leastAccesed);
        }

        public async Task<bool> UpdateFileMetadata(FileMetadata fileMetadata)
        {
            DocumentReference documentReference = _db.Collection(CollectionNames.FilesMetadata.ToString()).Document(fileMetadata.FileName);
            Dictionary<string, object> fileMetadataDictionary = new Dictionary<string, object>
            {
                { "FirebaseStorageID", fileMetadata.FirebaseStorageID.ToString() },
                { "LeastAccesed", fileMetadata.LeastAccesed.ToUniversalTime() }
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
            var previousModelFileMetadata = await GetFileMetadata("previous_mnist_model");
            if (previousModelFileMetadata == null)
            {
                Console.WriteLine("Previous model not found");
                return false;
            }

            var currentModelFileMetadata = await GetFileMetadata("current_mnist_model");
            if (currentModelFileMetadata == null)
            {
                Console.WriteLine("Current model not found");
                return false;
            }

            var previousModelFileMetadataUpdate = new FileMetadata("previous_mnist_model", Guid.Parse(currentModelFileMetadata.FirebaseStorageID), DateTime.Now);
            var currentModelFileMetadataUpdate = new FileMetadata("current_mnist_model", Guid.Parse(latestModelFirebaseStorageID), DateTime.Now);

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
