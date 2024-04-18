namespace RestApi.DTOS
{
    public record LogItemDto(string MicroserviceName);
    //public record TokenGenerationRequest(string Email, Dictionary<string, JsonElement> Claims);
    public class TokenGenerationRequest
    {
        public string? Email { get; set; }
        public Dictionary<string, object> Claims { get; set; } = new Dictionary<string, object>();
    }

    // ce dracului nu merge ala
}
