
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


    }
}
