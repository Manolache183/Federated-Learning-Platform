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

    }
}
