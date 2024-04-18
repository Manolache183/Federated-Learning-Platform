using System.Text.Json;

namespace Authenticator.DTOS
{
    public record TokenGenerationRequest(string Email, Dictionary<string, JsonElement> Claims);
}
