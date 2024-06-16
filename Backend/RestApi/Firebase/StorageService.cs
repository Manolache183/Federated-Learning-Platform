using Firebase.Storage;
using RestApi.Common;

namespace RestApi.Firebase
{
    public class StorageService
    {
        private readonly string _path = AppDomain.CurrentDomain.BaseDirectory + @"federated-learning-platform-secrets.json";
        private const string _bucket = "federated-learning-platf-c15c0.appspot.com";
        private const string _aggregatedModelsFolder = "aggregatedModels";
        private const string _clientModelsFolder = "clientModels";
        private readonly FirebaseStorage _db;
        private readonly HttpClient _storageHttpClient;

        public StorageService(HttpClient httpClient)
        {
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", _path);
            _db = new FirebaseStorage(_bucket);
            _storageHttpClient = httpClient;
        }

        public async Task<bool> UploadClientModel(Stream model, string fileName)
        {
            string modelCloudPath;
            try
            {
                modelCloudPath = await _db.Child(_clientModelsFolder).Child(fileName).PutAsync(model);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            Console.WriteLine("Model uploaded successfully to path: " + modelCloudPath);
            return true;
        }

        public async Task<string> PrintModel(string fileName)
        {
            var content = await DownloadModel(fileName);
            Console.WriteLine("File contents:");
            Console.WriteLine(content);

            return content;
        }

        public async Task<string> DownloadModel(string fileName)
        {
            var downloadUrl = await GetAggregatedModelFileUrl(fileName);
            
            var response = await _storageHttpClient.GetAsync(downloadUrl);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Failed to download file");
                return string.Empty;
            }
             
            var content = await response.Content.ReadAsStringAsync();
            return content;
        }
        
        public async Task<string?> GetAggregatedModelFileUrl(string fileName)
        {
            string? downloadUrl = null;
            try
            {
                downloadUrl = await _db.Child(_aggregatedModelsFolder).Child(fileName).GetDownloadUrlAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return downloadUrl;
        }

        public async Task<bool> DeleteModel(string fileName)
        {
            try
            {
                await _db.Child(_aggregatedModelsFolder).Child(fileName).DeleteAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            Console.WriteLine("Model deleted successfully");
            return true;
        }

        public async Task<bool> CleanupClientModels(string clientModelNamePrefix)
        {
            for (int i = 1; ; i++)
            {
                var clientModelName = clientModelNamePrefix + i;
                try
                {
                    await _db.Child(_clientModelsFolder).Child(clientModelName).DeleteAsync();
                    Console.WriteLine("Deleted model: " + clientModelName);
                }
                catch
                {
                    Console.WriteLine(i-1 + " client models deleted");
                    break;
                }
            }

            return true;
        }
    }
}
