using RestApi.DTOS;
using System.Text;
using System.Text.Json;

namespace RestApi.HttpClients
{
    public class LoggerService : ILoggerService
    {
        private readonly HttpClient _httpClient;
        private const string _microserviceName = "LearningCoordonator"; 

        public LoggerService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
     
        public async Task<HttpResponseMessage> PingAsync()
        {
            return await _httpClient.GetAsync("/logs");
        }

        public async Task<HttpResponseMessage> LogAsync()
        {
            var logItem = new LogItemDto(_microserviceName);
            var postData = new StringContent(JsonSerializer.Serialize(logItem), Encoding.UTF8, "application/json");
            return await _httpClient.PostAsync("/logs/logLearningCoordonator", postData);
        }

        public async Task<bool> AddFileMetadata(string fileName, Guid FirebaseStorageID)
        {
            var fileMetadata = new FileMetadataSend(fileName, FirebaseStorageID);
            var url = "/logs/addFileMetadata";
            var postData = new StringContent(JsonSerializer.Serialize(fileMetadata), Encoding.UTF8, "application/json");

            var httpResponseMessage = await _httpClient.PostAsync(url, postData);
            return httpResponseMessage.IsSuccessStatusCode;
        }

        public async Task<FileMetadata?> GetFileMetadata(string fileName)
        {
            var url = $"/logs/getFileMetadata/{fileName}";
            
            var httpResponseMessage = await _httpClient.GetAsync(url);
            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                return null;
            }

            var content = await httpResponseMessage.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<FileMetadata>(content);
        }

        public async Task<bool> UpdateFileMetadata(string fileName, string filePath, Guid FirebaseStorageID)
        {
            var fileMetadata = new FileMetadataSend(fileName, FirebaseStorageID);
            var url = "/logs/updateFileMetadata";
            var putData = new StringContent(JsonSerializer.Serialize(fileMetadata), Encoding.UTF8, "application/json");
            
            var httpResponseMessage = await _httpClient.PutAsync(url, putData);
            return httpResponseMessage.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteFileMetadata(string fileName)
        {
            var url = $"/logs/deleteFileMetadata/{fileName}";

            var httpResponseMessage = await _httpClient.DeleteAsync(url);
            return httpResponseMessage.IsSuccessStatusCode;       
        }

        public async Task<bool> SwapModelFiles(string latestModelFirebaseStorageID)
        {
            var url = $"/logs/swapModelFiles/{latestModelFirebaseStorageID}";

            var httpResponseMessage = await _httpClient.PutAsync(url, null);
            return httpResponseMessage.IsSuccessStatusCode;
        }
    }
}
