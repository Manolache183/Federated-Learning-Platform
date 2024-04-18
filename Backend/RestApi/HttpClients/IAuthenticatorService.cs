using RestApi.DTOS;

namespace RestApi.HttpClients
{
    public interface IAuthenticatorService
    {
        public abstract Task<HttpResponseMessage> GetJwtAsync(TokenGenerationRequest tokenGenerationRequest);
    }
}
