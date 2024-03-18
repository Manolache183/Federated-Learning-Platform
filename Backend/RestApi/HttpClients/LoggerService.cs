using Newtonsoft.Json;
using RestApi.DTOS;
using System.Text;

namespace RestApi.HttpClients
{
    public class LoggerService : ILoggerService
    {
        private readonly HttpClient _httpClient;

        public LoggerService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        
        public async Task<HttpResponseMessage> LogAsync()
        {
            var logItem = new LogItem("LearningCoordonator", DateTime.Now);
            var stringPayload = JsonConvert.SerializeObject(logItem);
            
            using var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");
            
            return await _httpClient.PostAsync("/logs/logLearningCoordonator", httpContent);
        }
    }
}
