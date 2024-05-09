using RestApi.DTOS;

namespace RestApi.HttpClients
{
    public interface IAuthenticatorService
    {
        Task<bool> RegisterUser(UserData userData);
        Task<string?> GenerateJWT(UserData userData);
        Task<bool> DeleteUser(string email);
    }
}
