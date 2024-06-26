﻿using Newtonsoft.Json;
using RestApi.Common;
using System.Text;

namespace RestApi.HttpClients
{
    public class ClientPlatformService : IClientPlatformService
    {
        private readonly HttpClient _httpClient;

        public ClientPlatformService(HttpClient httpClient) {
            _httpClient = httpClient;
        }

        public async Task NotifyClient(string clientID, TrainingInfo trainingInfo)
        {
            var url = $"/api/projects/{clientID}/trainingRounds";
            var postData = JsonConvert.SerializeObject(trainingInfo);
            var content = new StringContent(postData, Encoding.UTF8, "application/json");

            Console.WriteLine("Notifying client: " + trainingInfo.deviceCount + " " + trainingInfo.accuracy + " " + trainingInfo.startedAt + " " + trainingInfo.finishedAt);

            try
            {
                var response = await _httpClient.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Client notified successfully");
                }
                else
                {
                    Console.WriteLine($"Failed to notify client. Status code: {response.StatusCode}, Reason: {response.ReasonPhrase}");
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Request exception: {e.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unexpected error: {e.Message}");
            }
        }

        public async Task<int> GetClientTrainingInterval(string clientID)
        {
            Console.WriteLine("Getting client timestamp");

            var url = $"/api/projects/{clientID}";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                dynamic project = JsonConvert.DeserializeObject(content);

                int timestamp = project.trainingRoundIntervalMinutes;
                Console.WriteLine("Client timestamp: " + timestamp);

                return timestamp;
            }
            
            Console.WriteLine($"Failed to get client timestamp. Status code: {response.StatusCode}, Reason: {response.ReasonPhrase}");
            
            return -1;
        }

        public async Task<int> GetClientThreshold(string clientID)
        {
            var url = $"/api/projects/{clientID}";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                dynamic project = JsonConvert.DeserializeObject(content);

                int threshold = project.minAvailableClients;
                Console.WriteLine("Client threshold: " + threshold);

                return threshold;
            }

            return -1;
        }
    }
}
