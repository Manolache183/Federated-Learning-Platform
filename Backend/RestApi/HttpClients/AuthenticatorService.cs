
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

        public async Task<bool> DeleteUser(string email)
        {
            var url = $"auth/deleteUser/{email}";
            var httpResponseMessage = await _httpClient.DeleteAsync(url);
            
            return httpResponseMessage.IsSuccessStatusCode;
        }

        public async Task<string?> GenerateJWT(UserData userData)
        {
            var url = "auth/generateToken";

            var postData = new StringContent(JsonSerializer.Serialize(userData), Encoding.UTF8, "application/json");
            var httpResponseMessage = await _httpClient.PostAsync(url, postData);

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                return null;
            }

            var content = await httpResponseMessage.Content.ReadAsStringAsync();
            var userToken = JsonSerializer.Deserialize<UserToken>(content);

            return userToken?.token;
        }

        public async Task<bool> RegisterUser(UserData userData)
        {
            var url = "auth/registerUser";
            var postData = new StringContent(JsonSerializer.Serialize(userData), Encoding.UTF8, "application/json");
            var httpResponseMessage = await _httpClient.PostAsync(url, postData);

            return httpResponseMessage.IsSuccessStatusCode;
        }
    }
}
