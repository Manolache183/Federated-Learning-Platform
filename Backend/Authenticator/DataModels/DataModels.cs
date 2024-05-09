using System.Text.Json;

namespace Authenticator.DataModels
{
    public record UserData(string email, string password, string role);
    public record UserToken(string token);
}
