
using RestApi.DTOS;
using System.Text;
using System.Text.Json;

namespace RestApi.HttpClients
{
    public class AuthenticatorService : IAuthenticatorService
    {
        private readonly HttpClient _httpClient;

        public AuthenticatorService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<HttpResponseMessage> GetJwtAsync(TokenGenerationRequest tokenGenerationRequest)
        {
            var payload = JsonSerializer.Serialize(tokenGenerationRequest);
            using var httpContent = new StringContent(payload, Encoding.UTF8, "application/json");
            return await _httpClient.PostAsync("getJwtToken", httpContent);
        }
    }
}
